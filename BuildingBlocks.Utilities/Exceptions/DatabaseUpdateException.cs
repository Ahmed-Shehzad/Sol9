using FluentResults;

namespace BuildingBlocks.Utilities.Exceptions;

/// <summary>
/// Represents an exception that occurs during database updates.
/// </summary>
public class DatabaseUpdateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseUpdateException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DatabaseUpdateException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseUpdateException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public DatabaseUpdateException(string message, Exception exception) : base(message, exception) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseUpdateException"/> class with a specified error message and a <see cref="FluentResults.Error"/> object.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="error">The <see cref="FluentResults.Error"/> object that represents the error.</param>
    public DatabaseUpdateException(string message, Error error) : base(message) { }
}