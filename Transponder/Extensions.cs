using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Transponder.Abstractions;
using Transponder.Persistence.Abstractions;
using Transponder.Transports.Abstractions;

namespace Transponder;

/// <summary>
/// Extension methods to register Transponder services.
/// </summary>
public static class Extensions
{
    public static IServiceCollection AddTransponder(
        this IServiceCollection services,
        Uri address,
        Action<TransponderBusOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(address);

        var options = new TransponderBusOptions(address);
        configure?.Invoke(options);

        return services.AddTransponder(options);
    }

    public static IServiceCollection AddTransponder(
        this IServiceCollection services,
        TransponderBusOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        if (options.TransportBuilder.HasRegistrations)
            options.TransportBuilder.Apply(services);

        services.TryAddSingleton<IMessageSerializer, JsonMessageSerializer>();

        if (options.OutboxOptions is not null)
        {
            services.AddSingleton(options.OutboxOptions);
            services.AddSingleton<OutboxDispatcher>();
        }

        services.AddSingleton(sp =>
        {
            Func<Type, Uri?> resolver = options.RequestAddressResolver
                                        ?? TransponderRequestAddressResolver.Create(
                                            options.Address,
                                            options.RequestPathPrefix,
                                            options.RequestPathFormatter);

            Func<IServiceProvider, TransponderBus, IMessageScheduler> schedulerFactory = options.SchedulerFactory
                                                                                         ?? ((_, bus) => new InMemoryMessageScheduler(bus));

            return new TransponderBus(
                options.Address,
                sp.GetRequiredService<ITransportHostProvider>(),
                sp.GetServices<ITransportHost>(),
                sp.GetRequiredService<IMessageSerializer>(),
                resolver,
                options.DefaultRequestTimeout,
                bus => schedulerFactory(sp, bus),
                sp.GetServices<IReceiveEndpoint>(),
                sp.GetService<OutboxDispatcher>(),
                sp.GetServices<ITransponderMessageScopeProvider>());
        });

        services.AddSingleton<IBus>(sp => sp.GetRequiredService<TransponderBus>());
        services.AddSingleton<IBusControl>(sp => sp.GetRequiredService<TransponderBus>());
        services.AddSingleton<IClientFactory>(sp => sp.GetRequiredService<TransponderBus>());
        services.AddSingleton<IPublishEndpoint>(sp => sp.GetRequiredService<TransponderBus>());
        services.AddSingleton<ISendEndpointProvider>(sp => sp.GetRequiredService<TransponderBus>());
        services.AddSingleton<IMessageScheduler>(sp => sp.GetRequiredService<TransponderBus>());

        return services;
    }

    public static TransponderBusOptions UsePersistedMessageScheduler(
        this TransponderBusOptions options,
        Action<PersistedMessageSchedulerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var schedulerOptions = new PersistedMessageSchedulerOptions();
        configure?.Invoke(schedulerOptions);

        options.SchedulerFactory = (sp, bus) => new PersistedMessageScheduler(
            bus,
            sp.GetRequiredService<IScheduledMessageStore>(),
            sp.GetRequiredService<IMessageSerializer>(),
            schedulerOptions);

        return options;
    }

    public static TransponderBusOptions UseOutbox(
        this TransponderBusOptions options,
        Action<OutboxDispatchOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var outboxOptions = new OutboxDispatchOptions();
        configure?.Invoke(outboxOptions);
        options.OutboxOptions = outboxOptions;
        return options;
    }
}
