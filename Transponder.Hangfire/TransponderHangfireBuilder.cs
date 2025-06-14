using Microsoft.Extensions.DependencyInjection;
using Transponder.Core.Abstractions;

namespace Transponder.Hangfire;

public class TransponderHangfireBuilder
{
    private readonly IServiceCollection _services;

    public TransponderHangfireBuilder(IServiceCollection services)
    {
        _services = services;
    }
    
    public void RegisterHangfireJobs(string connectionString)
    {
        _services.AddScoped<IOutboxProcessorJob, OutboxProcessorJob>();
    }
}