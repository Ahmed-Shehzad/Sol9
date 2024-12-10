using FluentResults;

namespace BuildingBlocks.Utilities.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a request is not valid or cannot be processed.
/// </summary>
public class BadRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class.
    /// </summary>
    public BadRequestException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BadRequestException(string message) : base(message) { }
    public BadRequestException(string message, Error error) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public BadRequestException(string message, Exception innerException)
        : base(message, innerException) { }
}