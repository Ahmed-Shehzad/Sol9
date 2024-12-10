using FluentResults;

namespace BuildingBlocks.Utilities.Exceptions;

public class DatabaseUpdateException : Exception
{
    public DatabaseUpdateException(string message) : base(message) { }
    public DatabaseUpdateException(string message, Error error) : base(message) { }
}