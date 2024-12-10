namespace Orders.Domain.Aggregates.Entities.ValueObjects;

public record DocumentInfo
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public required string Url { get; init; }
}