using Sol9.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddDnsDestinationResolver();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapReverseProxy();

app.Run();
