using System;

using Xunit;

namespace Orders.E2E.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class E2EFactAttribute : FactAttribute
{
    private const string BaseUrlEnv = "E2E_BASE_URL";

    public E2EFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(BaseUrlEnv)))
            Skip = $"Set {BaseUrlEnv} to the API gateway base URL (e.g. http://localhost:18080) to run E2E tests.";
    }
}
