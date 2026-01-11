using System.Net;
using System.Text.Json;

using Microsoft.Playwright;

using Xunit;

namespace Orders.E2E.Tests;

public sealed class OrdersApiTests : IAsyncLifetime
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

    [E2EFact]
    public async Task GetOrders_ReturnsCollectionWithLinksAsync()
    {
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

    [E2EFact]
    public async Task CreateOrder_ReturnsResourceWithLinksAsync()
    {
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

    private static string GetBaseUrl()
    {
        string? baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnv);
        return string.IsNullOrWhiteSpace(baseUrl)
            ? throw new InvalidOperationException(
                $"Set {BaseUrlEnv} to the API gateway base URL (e.g. http://localhost:18080) to run E2E tests.")
            : baseUrl.TrimEnd('/');
    }
    private async static Task<JsonElement> ReadJsonAsync(IAPIResponse response)
    {
        string body = await response.TextAsync().ConfigureAwait(false);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.Clone();
    }
}
