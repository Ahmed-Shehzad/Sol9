using System.Text.Json.Serialization;

namespace Sol9.Core.Hypermedia;

public sealed record PaginationMeta(
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("totalPages")] int TotalPages);
