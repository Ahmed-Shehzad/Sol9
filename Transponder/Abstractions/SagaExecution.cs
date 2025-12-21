using Transponder.Persistence.Abstractions;

namespace Transponder.Abstractions;

/// <summary>
/// Executes saga steps forward and compensates backward on failure.
/// </summary>
public static class SagaExecution
{
    public async static Task<SagaStatus> ExecuteAsync<TState>(
        SagaStyle style,
        TState state,
        IEnumerable<SagaStep<TState>> steps,
        CancellationToken cancellationToken = default)
        where TState : class, ISagaState
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(steps);

        if (style is not (SagaStyle.Orchestration or SagaStyle.Choreography)) throw new ArgumentOutOfRangeException(nameof(style), style, "Saga steps only apply to orchestration or choreography.");

        SetStatus(state, SagaStatus.Running);

        var executedSteps = new List<SagaStep<TState>>();

        try
        {
            foreach (SagaStep<TState> step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await step.ExecuteAsync(state, cancellationToken).ConfigureAwait(false);
                executedSteps.Add(step);
            }

            SetStatus(state, SagaStatus.Completed);
            return SagaStatus.Completed;
        }
        catch
        {
            SetStatus(state, SagaStatus.Compensating);

            bool compensationFailed = false;
            for (int index = executedSteps.Count - 1; index >= 0; index--)
                try
                {
                    await executedSteps[index].CompensateAsync(state, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    compensationFailed = true;
                }

            SagaStatus status = compensationFailed ? SagaStatus.Failed : SagaStatus.Compensated;
            SetStatus(state, status);
            return status;
        }
    }

    private static void SetStatus<TState>(TState state, SagaStatus status)
        where TState : class, ISagaState
    {
        if (state is ISagaStatusState statusState) statusState.Status = status;
    }
}
