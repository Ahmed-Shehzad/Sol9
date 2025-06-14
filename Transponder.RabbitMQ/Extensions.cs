using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Transponder.Core.Abstractions;

namespace Transponder.RabbitMQ;

public static class Extensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, Action<TransponderBuilder> configure)
    {
        var builder = new TransponderBuilder(services);
        configure(builder);
        builder.Build();

        var assemblies = builder.GetAssemblies();

        if (assemblies.Count == 0) return services;
        
        var notificationTypes = assemblies.SelectMany(x => 
            x.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IIntegrationEvent).IsAssignableFrom(t)));
        
        // Register open generic consumer
        services.AddScoped(typeof(IBusConsumer<>), typeof(BackgroundConsumerService<>));
        
        foreach (var notificationType in notificationTypes)
        {
            var consumerType = typeof(BackgroundConsumerService<>).MakeGenericType(notificationType);

            services.AddScoped(typeof(IBusConsumer<>).MakeGenericType(notificationType), consumerType);
            services.AddSingleton(typeof(IHostedService), consumerType);
        }
        
        services.AddScoped<IBusPublisher, RabbitMqBusPublisher>();
        
        return services;
    }
}