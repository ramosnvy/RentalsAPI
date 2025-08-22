using Serilog;
using Microsoft.EntityFrameworkCore;
using Rentals.Infrastructure; // seu DbContext vai estar aqui

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

// DbContext com Postgres
builder.Services.AddDbContext<RentalsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RentalsDb")));

// Add controllers + swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("RentalsDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<RentalsDbContext>();
        await context.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while ensuring database is created");
    }
}

// Middlewares
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