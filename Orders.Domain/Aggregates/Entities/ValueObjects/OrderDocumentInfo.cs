namespace Orders.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents information about a document, such as its name, type, and URL.
/// </summary>
public record OrderDocumentInfo
{
    /// <summary>
    /// Represents information about a document, such as its name, type, and URL.
    /// </summary>
    private OrderDocumentInfo(string name, string type, string url)
    {
        Name = name;
        Type = type;
        Url = url;
    }
    public static OrderDocumentInfo Create(string name, string type, string url)
    {
        return new OrderDocumentInfo(name, type, url);
    }

    /// <summary>
    /// Gets or sets the name of the document.
    /// </summary>
    /// <value>The name of the document.</value>
    public string Name { get; init; }

    /// <summary>
    /// Gets or sets the type of the document.
    /// </summary>
    /// <value>The type of the document.</value>
    public string Type { get; init; }

    /// <summary>
    /// Gets or sets the URL of the document.
    /// </summary>
    /// <value>The URL of the document.</value>
    public string Url { get; init; }

}