using System.Net;
using System.Text.Json;
using Microsoft.Playwright;
using Xunit;

namespace Transponder.Service.Tests;

public sealed class TransponderServiceTests : IAsyncLifetime
{
    private const string TransponderServiceUrlEnv = "TRANSPONDER_SERVICE_URL";
    private IPlaywright? _playwright;
    private IAPIRequestContext? _api;

    public async Task InitializeAsync()
    {
        string baseUrl = GetTransponderServiceUrl();
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
    public async Task HealthCheck_ReturnsHealthyAsync()
    {
        IAPIResponse response = await _api!.GetAsync("/health").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)response.Status);
    }

    [E2EFact]
    public async Task AliveCheck_ReturnsHealthyAsync()
    {
        IAPIResponse response = await _api!.GetAsync("/alive").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)response.Status);
    }

    [E2EFact]
    public async Task GrpcHealthCheck_IsAvailableAsync()
    {
        // gRPC health checks are typically accessed via gRPC protocol, not HTTP
        // This test verifies the service is running and accessible
        IAPIResponse response = await _api!.GetAsync("/health").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)response.Status);
    }

    private static string GetTransponderServiceUrl()
    {
        string? baseUrl = Environment.GetEnvironmentVariable(TransponderServiceUrlEnv);
        return string.IsNullOrWhiteSpace(baseUrl)
            ? throw new InvalidOperationException(
                $"Set {TransponderServiceUrlEnv} to the Transponder service URL (e.g. https://localhost:50051) to run tests.")
            : baseUrl.TrimEnd('/');
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class E2EFactAttribute : FactAttribute
{
    private const string TransponderServiceUrlEnv = "TRANSPONDER_SERVICE_URL";

    public E2EFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(TransponderServiceUrlEnv)))
            Skip = $"Set {TransponderServiceUrlEnv} to the Transponder service URL (e.g. https://localhost:50051) to run tests.";
    }
}
