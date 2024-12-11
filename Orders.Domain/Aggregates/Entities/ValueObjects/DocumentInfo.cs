namespace Orders.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents information about a document, such as its name, type, and URL.
/// </summary>
public record DocumentInfo(string Name, string Type, string Url)
{
    /// <summary>
    /// Gets or sets the name of the document.
    /// </summary>
    /// <value>The name of the document.</value>
    public required string Name { get; init; } = Name;

    /// <summary>
    /// Gets or sets the type of the document.
    /// </summary>
    /// <value>The type of the document.</value>
    public required string Type { get; init; } = Type;

    /// <summary>
    /// Gets or sets the URL of the document.
    /// </summary>
    /// <value>The URL of the document.</value>
    public required string Url { get; init; } = Url;
}