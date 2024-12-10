namespace BuildingBlocks.Extensions.Types;

public static class TaskExtensions
{
    /// <summary>
    /// Extends the functionality of the <see cref="Task"/> class by adding a delay method.
    /// This method will delay the execution of the provided task by the specified number of milliseconds.
    /// </summary>
    /// <param name="task">The task to be delayed.</param>
    /// <param name="millisecondsDelay">The number of milliseconds to delay the execution.</param>
    /// <returns>
    /// A task that represents the combined execution of the delay and the provided task.
    /// The returned task will complete when both the delay and the provided task have completed.
    /// </returns>
    public static async Task Delay(this Task task, int millisecondsDelay)
    {
        await Task.Delay(millisecondsDelay);
        await task;
    }
}