using Microsoft.Extensions.DependencyInjection;

namespace Transponder;

/// <summary>
/// Extension methods for registering saga support.
/// </summary>
public static class SagaExtensions
{
    public static IServiceCollection UseSagaOrchestration(
        this IServiceCollection services,
        Action<SagaRegistrationBuilder> configure)
        => UseSagas(services, Transponder.Abstractions.SagaStyle.Orchestration, configure);

    public static IServiceCollection UseSagaChoreography(
        this IServiceCollection services,
        Action<SagaRegistrationBuilder> configure)
        => UseSagas(services, Transponder.Abstractions.SagaStyle.Choreography, configure);

    private static IServiceCollection UseSagas(
        IServiceCollection services,
        Transponder.Abstractions.SagaStyle style,
        Action<SagaRegistrationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new SagaRegistrationBuilder(services, style);
        configure(builder);
        services.AddSingleton(builder.Build());
        return services;
    }
}
