namespace Transponder;

/// <summary>
/// Configures the persisted message scheduler.
/// </summary>
public sealed class PersistedMessageSchedulerOptions
{
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);

    public int BatchSize { get; set; } = 100;
}
