using System.Text.Json.Serialization;
using BuildingBlocks.Domain.Aggregates.Dtos;

namespace Orders.Application.Dtos;

public record TimeFrameDto(
    [property: JsonPropertyName("dayOfWeek")]
    DayOfWeek DayOfWeek,
    TimeOnly From,
    TimeOnly To) : RangeDto<TimeOnly>(From,
    To);