
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Controllers;
using PrivateChildcareCalendarApi.Middleware;
using PrivatPasningKalender.Data;
using PrivatPasningKalender.Infrastructure;
using PrivatPasningKalender.Services;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

namespace PrivateChildcareCalendarApi;

public class Program
{
    private const string CorsPolicy = "SvelteKit";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration
                .AddJsonFile(
                    "local.settings.json",
                    optional: true,
                    reloadOnChange: true
                );
        }

        ConfigureServices(builder);
        ConfigureCors(builder);

        var app = builder.Build();

        InitializeDatabase(app);
        ConfigurePipeline(app);

        app.Run();
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Undgå cirkulære referencer (f.eks. ChildDayStatus.Child)
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        builder.Services.AddOpenApi();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new NullReferenceException("Database connection string is not configured.");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        builder.Services.AddScoped<ChildCapacityValidator>();
        builder.Services.AddScoped<CalendarEventService>();
    }

    public static void ConfigureCors(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "Privat Pasning API";
            });
        }

        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowAllOrigins");
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers();
    }

    public static void InitializeDatabase(WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DatabaseInitializer.Initialize(db);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Fejl under startup-initialisering.");
            throw;
        }
    }
}
