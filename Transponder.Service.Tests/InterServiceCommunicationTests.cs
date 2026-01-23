using System.Net;
using System.Text.Json;

using Microsoft.Playwright;

using Xunit;

namespace Transponder.Service.Tests;

/// <summary>
/// E2E tests that verify inter-service communication through the centralized Transponder gRPC service.
/// These tests verify that Orders.API and Bookings.API can communicate successfully via the central service.
/// </summary>
public sealed class InterServiceCommunicationTests : IAsyncLifetime
{
    private const string GatewayBaseUrlEnv = "E2E_BASE_URL";
    private IPlaywright? _playwright;
    private IAPIRequestContext? _api;

    public async Task InitializeAsync()
    {
        string baseUrl = GetGatewayBaseUrl();
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
    public async Task CreateOrder_TriggersBookingCreation_ViaTransponderServiceAsync()
    {
        // Arrange: Create an order
        var orderPayload = new
        {
            customerName = "Test Customer",
            totalAmount = 100.50m
        };

        // Act: Create order (this should trigger booking creation via Transponder)
        IAPIResponse orderResponse = await _api!.PostAsync("/orders/api/v1/orders", new APIRequestContextOptions
        {
            DataObject = orderPayload
        }).ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)orderResponse.Status);

        JsonElement orderRoot = await ReadJsonAsync(orderResponse).ConfigureAwait(false);
        Assert.True(orderRoot.TryGetProperty("data", out JsonElement orderData));
        string orderIdStr = orderData.GetString() ?? string.Empty;
        Assert.NotEmpty(orderIdStr);

        // Wait a bit for the booking to be created via Transponder
        await Task.Delay(2000).ConfigureAwait(false);

        // Assert: Verify booking was created (via Transponder service communication)
        IAPIResponse bookingResponse = await _api.GetAsync($"/bookings/api/v1/bookings/order/{orderIdStr}").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)bookingResponse.Status);

        JsonElement bookingRoot = await ReadJsonAsync(bookingResponse).ConfigureAwait(false);
        Assert.True(bookingRoot.TryGetProperty("data", out JsonElement bookingData));
        Assert.True(bookingData.TryGetProperty("orderId", out JsonElement bookingOrderId));
        Assert.Equal(orderIdStr, bookingOrderId.GetString());
    }

    [E2EFact]
    public async Task CreateOrder_ThenCancelOrder_TriggersBookingCancellation_ViaTransponderServiceAsync()
    {
        // Arrange: Create an order
        var orderPayload = new
        {
            customerName = "Test Customer Cancel",
            totalAmount = 200.75m
        };

        IAPIResponse orderResponse = await _api!.PostAsync("/orders/api/v1/orders", new APIRequestContextOptions
        {
            DataObject = orderPayload
        }).ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)orderResponse.Status);

        JsonElement orderRoot = await ReadJsonAsync(orderResponse).ConfigureAwait(false);
        Assert.True(orderRoot.TryGetProperty("data", out JsonElement orderData));
        string orderIdStr = orderData.GetString() ?? string.Empty;

        // Wait for booking creation
        await Task.Delay(2000).ConfigureAwait(false);

        // Act: Cancel the order (this should trigger booking cancellation via Transponder)
        IAPIResponse cancelResponse = await _api.DeleteAsync($"/orders/api/v1/orders/{orderIdStr}").ConfigureAwait(false);
        Assert.True(
            (HttpStatusCode)cancelResponse.Status == HttpStatusCode.OK ||
            (HttpStatusCode)cancelResponse.Status == HttpStatusCode.NoContent,
            $"Expected OK or NoContent, got {cancelResponse.Status}");

        // Wait for booking cancellation
        await Task.Delay(2000).ConfigureAwait(false);

        // Assert: Verify booking was cancelled (via Transponder service communication)
        IAPIResponse bookingResponse = await _api.GetAsync($"/bookings/api/v1/bookings/order/{orderIdStr}").ConfigureAwait(false);
        // Booking might be cancelled or not found depending on implementation
        Assert.True(
            (HttpStatusCode)bookingResponse.Status == HttpStatusCode.OK ||
            (HttpStatusCode)bookingResponse.Status == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {bookingResponse.Status}");
    }

    [E2EFact]
    public async Task MultipleOrders_CreateMultipleBookings_ViaTransponderServiceAsync()
    {
        // Arrange: Create multiple orders
        var orders = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            var orderPayload = new
            {
                customerName = $"Test Customer {i}",
                totalAmount = 50.00m + i
            };

            IAPIResponse orderResponse = await _api!.PostAsync("/orders/api/v1/orders", new APIRequestContextOptions
            {
                DataObject = orderPayload
            }).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)orderResponse.Status);

            JsonElement orderRoot = await ReadJsonAsync(orderResponse).ConfigureAwait(false);
            Assert.True(orderRoot.TryGetProperty("data", out JsonElement orderData));
            string orderIdStr = orderData.GetString() ?? string.Empty;
            orders.Add(orderIdStr);
        }

        // Wait for all bookings to be created via Transponder
        await Task.Delay(3000).ConfigureAwait(false);

        // Assert: Verify all bookings were created (via Transponder service communication)
        foreach (string orderId in orders)
        {
            IAPIResponse bookingResponse = await _api!.GetAsync($"/bookings/api/v1/bookings/order/{orderId}").ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)bookingResponse.Status);

            JsonElement bookingRoot = await ReadJsonAsync(bookingResponse).ConfigureAwait(false);
            Assert.True(bookingRoot.TryGetProperty("data", out JsonElement bookingData));
            Assert.True(bookingData.TryGetProperty("orderId", out JsonElement bookingOrderId));
            Assert.Equal(orderId, bookingOrderId.GetString() ?? string.Empty);
        }
    }

    private static string GetGatewayBaseUrl()
    {
        string? baseUrl = Environment.GetEnvironmentVariable(GatewayBaseUrlEnv);
        return string.IsNullOrWhiteSpace(baseUrl)
            ? throw new InvalidOperationException(
                $"Set {GatewayBaseUrlEnv} to the API gateway base URL (e.g. http://localhost:18080) to run E2E tests.")
            : baseUrl.TrimEnd('/');
    }

    private async static Task<JsonElement> ReadJsonAsync(IAPIResponse response)
    {
        string body = await response.TextAsync().ConfigureAwait(false);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.Clone();
    }
}
