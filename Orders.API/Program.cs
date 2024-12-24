using System.Threading.RateLimiting;
using BuildingBlocks.ServiceCollection.Extensions;
using BuildingBlocks.Utilities.Middlewares;
using Treblle.Net.Core;

namespace Orders.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add Rate Limiting Configuration
        builder.Services.AddOptions(); // Needed for configuration setup

        // Add Memory Cache (required for rate limiting)
        builder.Services.AddMemoryCache();

        builder.Services.AddTreblle(
            builder.Configuration["Treblle:ApiKey"]!,
            builder.Configuration["Treblle:ProjectId"]!,
            "secretField,highlySensitiveField");

        builder.Services.AddRateLimiterConfigurations();

        // HSTS Security Headers 
        builder.Services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromHours(1);
        });

        var app = builder.Build();

        // // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
        //     app.UseSwagger();
        //     app.UseSwaggerUI();
        // }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHsts();

        app.UseHttpsRedirection();

        app.UseRateLimiter();

        app.UseMiddleware<AcceptHeaderMiddleware>();
        app.UseMiddleware<AllowHeaderMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        app.UseAuthorization();

        app.UseTreblle(useExceptionHandler: true);

        app.MapControllers();

        app.Run();
    }
}