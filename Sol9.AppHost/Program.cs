SetIfMissing("ASPNETCORE_URLS", "http://localhost:18888");
if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL")) &&
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL")))
{
    Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL", "http://localhost:4317");
    Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL", "http://localhost:4318");
}

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
_ = builder.AddKubernetesEnvironment("k8s");

IResourceBuilder<ParameterResource> pgUser = builder.AddParameter("postgres-user", "postgres");
IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("postgres-password", "postgres", secret: true);
IResourceBuilder<ParameterResource> redisPassword = builder.AddParameter("redis-password", "redis", secret: true);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres", pgUser, pgPassword, 5432)
    .WithDataBindMount("./.data/postgres");

IResourceBuilder<PostgresDatabaseResource> bookingsDb = postgres.AddDatabase("bookings");
IResourceBuilder<PostgresDatabaseResource> ordersDb = postgres.AddDatabase("orders");

IResourceBuilder<ContainerResource> redis = builder.AddContainer("redis", "redis:8.2")
    .WithEnvironment("REDIS_PASSWORD", redisPassword)
    .WithArgs("--requirepass", redisPassword, "--port", "6379")
    .WithEndpoint(targetPort: 6379, name: "tcp");

EndpointReference redisEndpoint = redis.GetEndpoint("tcp");
EndpointReferenceExpression redisHost = redisEndpoint.Property(EndpointProperty.Host);
EndpointReferenceExpression redisPort = redisEndpoint.Property(EndpointProperty.Port);

var redisConnection = ReferenceExpression.Create(
    $"{redisHost}:{redisPort},password={redisPassword}");

const string https = "https";

IResourceBuilder<ProjectResource> bookingsApi = builder.AddProject<Projects.Bookings_API>("bookings-api")
    .WithReference(bookingsDb)
    .WithReference(bookingsDb, "Transponder")
    .WithHttpsEndpoint()
    .WithEnvironment("ConnectionStrings__Redis", redisConnection)
    .WaitForStart(redis);

IResourceBuilder<ProjectResource> ordersApi = builder.AddProject<Projects.Orders_API>("orders-api")
    .WithReference(ordersDb)
    .WithReference(ordersDb, "Transponder")
    .WithHttpsEndpoint()
    .WithEnvironment("ConnectionStrings__Redis", redisConnection)
    .WaitForStart(redis);

// Each service hosts its own gRPC transport endpoint; share contracts, not runtime.
_ = bookingsApi.WithEnvironment("TransponderDefaults__LocalAddress", bookingsApi.GetEndpoint(https));
_ = bookingsApi.WithEnvironment("TransponderDefaults__RemoteAddress", ordersApi.GetEndpoint(https));

_ = ordersApi.WithEnvironment("TransponderDefaults__LocalAddress", ordersApi.GetEndpoint(https));
_ = ordersApi.WithEnvironment("TransponderDefaults__RemoteAddress", bookingsApi.GetEndpoint(https));

builder.AddProject<Projects.Gateway_API>("gateway-api")
    .WithReference(bookingsApi)
    .WithReference(ordersApi)
    .WithEnvironment(
        "ReverseProxy__Clusters__bookings-cluster__Destinations__bookings-1__Address",
        bookingsApi.GetEndpoint(https))
    .WithEnvironment(
        "ReverseProxy__Clusters__orders-cluster__Destinations__orders-1__Address",
        ordersApi.GetEndpoint(https));

await builder.Build().RunAsync();

static void SetIfMissing(string name, string value)
{
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
        Environment.SetEnvironmentVariable(name, value);
}
