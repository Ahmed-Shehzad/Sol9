using System.Net;
using System.Text.Json;

using Microsoft.Playwright;

using Xunit;
using Xunit.Sdk;

namespace Bookings.E2E.Tests;

public sealed class BookingsApiTests : IAsyncLifetime
{
    private const string BaseUrlEnv = "E2E_BASE_URL";
    private IPlaywright? _playwright;
    private IAPIRequestContext? _api;

    public async Task InitializeAsync()
    {
        string baseUrl = GetBaseUrl();
        _playwright = await Playwright.CreateAsync().ConfigureAwait(false);
        _api = await _playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = baseUrl
        }).ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        if (_api is not null)
            await _api.DisposeAsync().ConfigureAwait(false);
        _playwright?.Dispose();
    }

    [Fact]
    public async Task GetBookings_ReturnsCollectionWithLinksAsync()
    {
        IAPIResponse response = await _api!.GetAsync("/bookings/api/v1/bookings").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)response.Status);

        JsonElement root = await ReadJsonAsync(response).ConfigureAwait(false);
        Assert.True(root.TryGetProperty("items", out _));
        Assert.True(root.TryGetProperty("_links", out JsonElement links));
        Assert.True(links.TryGetProperty("self", out _));
        Assert.True(links.TryGetProperty("create", out _));
        Assert.True(root.TryGetProperty("meta", out JsonElement meta));
        Assert.True(meta.TryGetProperty("page", out _));
        Assert.True(meta.TryGetProperty("pageSize", out _));
        Assert.True(meta.TryGetProperty("totalCount", out _));
        Assert.True(meta.TryGetProperty("totalPages", out _));
    }

    [Fact]
    public async Task CreateBooking_ReturnsResourceWithLinksAsync()
    {
        var payload = new
        {
            orderId = Guid.NewGuid(),
            customerName = "Ada Lovelace"
        };

        IAPIResponse response = await _api!.PostAsync("/bookings/api/v1/bookings", new APIRequestContextOptions
        {
            DataObject = payload
        }).ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)response.Status);

        JsonElement root = await ReadJsonAsync(response).ConfigureAwait(false);
        Assert.True(root.TryGetProperty("data", out JsonElement data));
        Guid id = Guid.Parse(data.GetString() ?? string.Empty);
        Assert.NotEqual(Guid.Empty, id);
        Assert.True(root.TryGetProperty("_links", out JsonElement links));
        Assert.True(links.TryGetProperty("self", out _));
        Assert.True(links.TryGetProperty("byOrder", out _));
        Assert.True(links.TryGetProperty("collection", out _));

        IAPIResponse getById = await _api.GetAsync($"/bookings/api/v1/bookings/{id}").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)getById.Status);

        IAPIResponse getByOrder = await _api.GetAsync($"/bookings/api/v1/bookings/order/{payload.orderId}").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)getByOrder.Status);
    }

    private static string GetBaseUrl()
    {
        string? baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnv);
        return string.IsNullOrWhiteSpace(baseUrl)
            ? throw SkipException.ForSkip(
                $"Set {BaseUrlEnv} to the API gateway base URL (e.g. http://localhost:18080) to run E2E tests.")
            : baseUrl.TrimEnd('/');
    }

    private static async Task<JsonElement> ReadJsonAsync(IAPIResponse response)
    {
        string body = await response.TextAsync().ConfigureAwait(false);
        using JsonDocument doc = JsonDocument.Parse(body);
        return doc.RootElement.Clone();
    }
}
