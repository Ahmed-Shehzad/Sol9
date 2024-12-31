using System.Text.Json.Serialization;

namespace BuildingBlocks.Domain.Aggregates.Dtos;

public abstract record BaseDto
{
    protected BaseDto(Ulid id)
    {
        Id = id;
    }
    
    [JsonPropertyName("id")]
    public Ulid? Id { get; init; }
}