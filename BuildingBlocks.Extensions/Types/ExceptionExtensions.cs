namespace BuildingBlocks.Extensions.Types;

public static class ExceptionExtensions
{
    /// <summary>
    ///     Unwraps the most inner exception.
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static Exception? Unwrap(this Exception? exception)
    {
        while (exception?.InnerException is not null)
        {
            exception = exception.InnerException;
        }
        return exception;
    }
}