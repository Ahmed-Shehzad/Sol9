SetIfMissing("ASPNETCORE_URLS", "http://localhost:18888");
SetIfMissing("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");
if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL")) &&
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL")))
{
    Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL", "http://localhost:4317");
    Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL", "http://localhost:4318");
}

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> pgUser = builder.AddParameter("postgres-user", "postgres");
IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("postgres-password", "postgres", secret: true);
IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres", pgUser, pgPassword, 5432);
IResourceBuilder<PostgresDatabaseResource> bookingsDb = postgres.AddDatabase("bookings");
IResourceBuilder<PostgresDatabaseResource> ordersDb = postgres.AddDatabase("orders");
IResourceBuilder<PostgresDatabaseResource> transponderDb = postgres.AddDatabase("transponder");

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis")
    .WithBindMount("certs/redis", "/tls", isReadOnly: true)
    .WithArgs(
        "--tls-port", "6379",
        "--port", "0",
        "--tls-cert-file", "/tls/redis.crt",
        "--tls-key-file", "/tls/redis.key",
        "--tls-ca-cert-file", "/tls/ca.crt",
        "--tls-auth-clients", "no");

ReferenceExpression redisHost = redis.Resource.GetConnectionProperty("Host");
ReferenceExpression redisPort = redis.Resource.GetConnectionProperty("Port");
var redisUri = ReferenceExpression.Create($"rediss://{redisHost}:{redisPort}");
var redisTlsConnection = ReferenceExpression.Create($"{redisHost}:{redisPort},ssl=true,sslHost=redis");
redis.WithConnectionProperty("Uri", redisUri);
IResourceBuilder<ConnectionStringResource> redisTls = builder.AddConnectionString("redis-tls", redisTlsConnection);
redis.WithConnectionStringRedirection(redisTls.Resource);

IResourceBuilder<ProjectResource> bookingsApi = builder.AddProject<Projects.Bookings_API>("bookings-api")
    .WithReference(bookingsDb)
    .WithReference(transponderDb)
    .WithEnvironment("ConnectionStrings__Redis", redisTlsConnection)
    .WaitFor(redis);

IResourceBuilder<ProjectResource> ordersApi = builder.AddProject<Projects.Orders_API>("orders-api")
    .WithReference(ordersDb)
    .WithReference(transponderDb)
    .WithEnvironment("ConnectionStrings__Redis", redisTlsConnection)
    .WaitFor(redis);

builder.AddProject<Projects.Gateway_API>("gateway-api")
    .WithReference(bookingsApi)
    .WithReference(ordersApi)
    .WithEnvironment(
        "ReverseProxy__Clusters__bookings-cluster__Destinations__bookings-1__Address",
        bookingsApi.GetEndpoint("http"))
    .WithEnvironment(
        "ReverseProxy__Clusters__orders-cluster__Destinations__orders-1__Address",
        ordersApi.GetEndpoint("http"));

await builder.Build().RunAsync();

static void SetIfMissing(string name, string value)
{
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
        Environment.SetEnvironmentVariable(name, value);
}
