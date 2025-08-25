using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rentals.Infrastructure;
using Rentals.Infrastructure.Repositories;
using Rentals.Infrastructure.Images;
using Rentals.Application.Abstractions;
using Rentals.Application.Commands;
using Minio;
using MongoDB.Driver;
using Rentals.Application.Queries;
using Rentals.Infrastructure.Auth;
using Serilog;
using ImageConverter = Rentals.Infrastructure.Images.ImageConverter;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

// DbContext
builder.Services.AddDbContext<RentalsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RentalsDb")));

// Repositories
builder.Services.AddScoped<IDeliveryDriverRepository, DeliveryDriverRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// MinIO client
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var config = builder.Configuration.GetSection("Minio");
    return new MinioClient()
        .WithEndpoint(config["Endpoint"]!.Replace("http://", "").Replace("https://", ""))
        .WithCredentials(config["AccessKey"], config["SecretKey"])
        .WithSSL(config["Endpoint"]!.StartsWith("https"))
        .Build();
});

// Storage service
builder.Services.AddScoped<IStorageService, MinioStorageService>();

// Handlers
builder.Services.AddScoped<RegisterDeliveryDriverHandler>();
builder.Services.AddScoped<UploadCnhImageDeliveryDriverHandler>();
builder.Services.AddScoped<GetAllDeliveryDriversHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<RegisterAdminHandler>();
builder.Services.AddScoped<RegisterMotorcycleHandler>();
builder.Services.AddScoped<GetAllMotorcyclesHandler>();
builder.Services.AddScoped<GetMotorcycleByIdHandler>();
builder.Services.AddScoped<GetMotorcycles2024Handler>();
builder.Services.AddScoped<UpdateMotorcycleLicensePlateHandler>();
builder.Services.AddScoped<DeleteMotorcycleHandler>();
builder.Services.AddScoped<Motorcycle2024NotificationHandler>();

// Rental Handlers
builder.Services.AddScoped<CreateRentalPlanHandler>();
builder.Services.AddScoped<GetAllRentalPlansHandler>();
builder.Services.AddScoped<CreateRentalHandler>();
builder.Services.AddScoped<GetRentalByIdHandler>();
builder.Services.AddScoped<ReturnRentalHandler>();

// --- JWT ---
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // evita remapeamento automático

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        signingKey.KeyId = "default-key"; // Mesmo Key ID usado na geração

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKeys = new List<SecurityKey> { signingKey }, // Usar IssuerSigningKeys em vez de IssuerSigningKey

            RoleClaimType = System.Security.Claims.ClaimTypes.Role, // garante role
            NameClaimType = System.Security.Claims.ClaimTypes.Name, // facilita pegar username
            ClockSkew = TimeSpan.FromMinutes(2) // tolerância no tempo
        };

        // logs de debug em caso de erro
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrWhiteSpace(ctx.Token))
                {
                    var logger = ctx.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtAuth");
                    logger.LogWarning("No bearer token found in request.");
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                logger.LogError(ctx.Exception, "JWT authentication failed.");
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                logger.LogWarning("JWT challenge: {Error} - {Description}", ctx.Error, ctx.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IImageConverter, ImageConverter>();

// MongoDB
builder.Services.Configure<Rentals.Infrastructure.MongoDB.MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDB").Get<Rentals.Infrastructure.MongoDB.MongoDbSettings>();
    return new MongoDB.Driver.MongoClient(settings?.ConnectionString ?? "mongodb://localhost:27017");
});

// Repositories
builder.Services.AddScoped<Rentals.Application.Abstractions.IMotorcycleRepository, Rentals.Infrastructure.Repositories.MotorcycleRepository>();
builder.Services.AddScoped<Rentals.Application.Abstractions.IMotorcycleNotificationRepository, Rentals.Infrastructure.MongoDB.Repositories.MongoMotorcycleNotificationRepository>();
builder.Services.AddScoped<Rentals.Application.Abstractions.IRentalPlanRepository, Rentals.Infrastructure.Repositories.RentalPlanRepository>();
builder.Services.AddScoped<Rentals.Application.Abstractions.IRentalRepository, Rentals.Infrastructure.Repositories.RentalRepository>();

// Services
builder.Services.AddScoped<Rentals.Application.Services.IRentalCalculationService, Rentals.Application.Services.RentalCalculationService>();

// Message Bus - Registrado como Singleton para ser opcional
builder.Services.AddSingleton<Rentals.Application.Abstractions.IMessageBus, Rentals.Infrastructure.Messaging.RabbitMQMessageBus>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- Swagger ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rentals API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,   // http (não ApiKey)
        Scheme = "bearer",                // minúsculo
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("RentalsDb") ?? "Host=localhost;Database=rentals;Username=postgres;Password=postgres");

var app = builder.Build();

// dev only
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RentalsDbContext>();
    db.Database.Migrate();
    
    // Inicializar planos padrão
    var calculationService = scope.ServiceProvider.GetRequiredService<Rentals.Application.Services.IRentalCalculationService>();
    await calculationService.InitializeDefaultPlans();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Configurar consumer para motos de 2024
using (var scope = app.Services.CreateScope())
{
    var messageBus = scope.ServiceProvider.GetRequiredService<Rentals.Application.Abstractions.IMessageBus>();
    var notificationHandler = scope.ServiceProvider.GetRequiredService<Rentals.Application.Commands.Motorcycle2024NotificationHandler>();

    await messageBus.SubscribeAsync<Rentals.Application.Commands.MotorcycleCreatedMessage>(
        "motorcycle.created",
        async message => await notificationHandler.Handle(message));
}

app.Run();
