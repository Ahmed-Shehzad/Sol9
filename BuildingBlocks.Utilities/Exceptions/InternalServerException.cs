using FluentResults;

namespace BuildingBlocks.Utilities.Exceptions;

/// <summary>
/// Represents an internal server exception.
/// </summary>
/// <remarks>
/// This class inherits from the base Exception class and provides constructors to initialize the exception with different parameters.
/// </remarks>
public class InternalServerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerException"/> class.
    /// </summary>
    public InternalServerException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InternalServerException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerException"/> class with a specified error message and an additional error object.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="error">An additional error object that provides more details about the exception.</param>
    public InternalServerException(string message, Error error) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public InternalServerException(string message, Exception innerException) : base(message, innerException) { }
}