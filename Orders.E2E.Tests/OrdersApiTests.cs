using System.Net;
using System.Text.Json;

using Microsoft.Playwright;

using Xunit;
using Xunit.Sdk;

namespace Orders.E2E.Tests;

public sealed class OrdersApiTests : IAsyncLifetime
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
    public async Task GetOrders_ReturnsCollectionWithLinksAsync()
    {
        SkipIfNotConfigured();
        IAPIResponse response = await _api!.GetAsync("/orders/api/v1/orders").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)response.Status);

        JsonElement root = await ReadJsonAsync(response).ConfigureAwait(true);
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
    public async Task CreateOrder_ReturnsResourceWithLinksAsync()
    {
        SkipIfNotConfigured();
        var payload = new
        {
            customerName = "Ada Lovelace",
            totalAmount = 123.45
        };

        IAPIResponse response = await _api!.PostAsync("/orders/api/v1/orders", new APIRequestContextOptions
        {
            DataObject = payload
        }).ConfigureAwait(true);

        Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)response.Status);

        JsonElement root = await ReadJsonAsync(response).ConfigureAwait(true);
        Assert.True(root.TryGetProperty("data", out JsonElement data));
        var id = Guid.Parse(data.GetString() ?? string.Empty);
        Assert.NotEqual(Guid.Empty, id);
        Assert.True(root.TryGetProperty("_links", out JsonElement links));
        Assert.True(links.TryGetProperty("self", out _));
        Assert.True(links.TryGetProperty("cancel", out _));
        Assert.True(links.TryGetProperty("collection", out _));

        IAPIResponse getResponse = await _api.GetAsync($"/orders/api/v1/orders/{id}").ConfigureAwait(true);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)getResponse.Status);
    }

    private string? GetBaseUrl()
    {
        string? baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnv);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _skipReason = $"Set {BaseUrlEnv} to the API gateway base URL (e.g. http://localhost:18080) to run E2E tests.";
            return null;
        }

        return baseUrl.TrimEnd('/');
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
