using Microsoft.EntityFrameworkCore;
using Rentals.Infrastructure;
using Rentals.Infrastructure.Repositories;
using Rentals.Infrastructure;
using Rentals.Application.Abstractions;
using Rentals.Application.Commands;
using Minio;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

// DbContext
builder.Services.AddDbContext<RentalsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RentalsDb")));

// Repositories
builder.Services.AddScoped<IDeliveryDriverRepository, DeliveryDriverRepository>();

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

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("RentalsDb"));

var app = builder.Build();

// Migrations automáticas (opcional, dev only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RentalsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();