using Transponder.Transports.Abstractions;

namespace Transponder.Transports;

public static class ReceiveEndpointFaultSettingsResolver
{
    public static ReceiveEndpointFaultSettings? Resolve(IReceiveEndpointConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (configuration is IReceiveEndpointConfigurationWithFaults withFaults &&
            withFaults.FaultSettings is not null)
        {
            return withFaults.FaultSettings;
        }

        if (configuration.Settings.TryGetValue(ReceiveEndpointSettingsKeys.FaultSettings, out var value) &&
            value is ReceiveEndpointFaultSettings faultSettings)
        {
            return faultSettings;
        }

        return null;
    }
}
