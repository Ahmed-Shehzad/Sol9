SetIfMissing("ASPNETCORE_URLS", "http://localhost:18888");
SetIfMissing("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");
if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL")) &&
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL")))
{
    Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL", "http://localhost:4317");
    Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL", "http://localhost:4318");
}

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres");
IResourceBuilder<PostgresDatabaseResource> bookingsDb = postgres.AddDatabase("bookings");
IResourceBuilder<PostgresDatabaseResource> ordersDb = postgres.AddDatabase("orders");
IResourceBuilder<PostgresDatabaseResource> transponderDb = postgres.AddDatabase("transponder");

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis");

IResourceBuilder<ProjectResource> bookingsApi = builder.AddProject<Projects.Bookings_API>("bookings-api")
    .WithReference(bookingsDb)
    .WithReference(transponderDb)
    .WithReference(redis);

IResourceBuilder<ProjectResource> ordersApi = builder.AddProject<Projects.Orders_API>("orders-api")
    .WithReference(ordersDb)
    .WithReference(transponderDb)
    .WithReference(redis);

builder.AddProject<Projects.Gateway_API>("gateway-api")
    .WithReference(bookingsApi)
    .WithReference(ordersApi)
    .WithEnvironment(
        "ReverseProxy__Clusters__bookings-cluster__Destinations__bookings-1__Address",
        bookingsApi.GetEndpoint("http"))
    .WithEnvironment(
        "ReverseProxy__Clusters__orders-cluster__Destinations__orders-1__Address",
        ordersApi.GetEndpoint("http"));

builder.Build().Run();

static void SetIfMissing(string name, string value)
{
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
        Environment.SetEnvironmentVariable(name, value);
}
