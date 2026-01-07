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
    private string? _skipReason;

    public async Task InitializeAsync()
    {
        string? baseUrl = GetBaseUrl();
        if (baseUrl is null)
            return;
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
        SkipIfNotConfigured();
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
        SkipIfNotConfigured();
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
        var id = Guid.Parse(data.GetString() ?? string.Empty);
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

    private string? GetBaseUrl()
    {
        string? baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnv);
        if (!string.IsNullOrWhiteSpace(baseUrl)) return baseUrl.TrimEnd('/');

        _skipReason = $"Set {BaseUrlEnv} to the API gateway base URL (e.g. http://localhost:18080) to run E2E tests.";
        return null;

    }

    private void SkipIfNotConfigured()
    {
        if (_skipReason is not null)
            throw SkipException.ForSkip(_skipReason);
    }

    private async static Task<JsonElement> ReadJsonAsync(IAPIResponse response)
    {
        string body = await response.TextAsync().ConfigureAwait(false);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.Clone();
    }
}
