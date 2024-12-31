using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Serilog.ILogger;
using Log = Serilog.Log;
using ContextLogger = Serilog.ContextLoggerConfigurationExtension;
using EnvironmentLogger = Serilog.EnvironmentLoggerConfigurationExtensions;

namespace BuildingBlocks.ServiceCollection.Extensions;

public static class LoggerConfigurationExtension
{
    /// <summary>
    ///     This function configures and returns a Serilog logger instance.
    ///     The logger is set up to log verbose messages, exclude Microsoft and System logs,
    ///     enrich log events with contextual information, and write logs to various sinks.
    /// </summary>
    /// <param name="environment">The web host environment. This parameter is optional and can be null.</param>
    /// <returns>An instance of the configured Serilog logger.</returns>
    public static ILogger GetSerilogLoggerConfiguration(IWebHostEnvironment? environment)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("System", LogEventLevel.Error)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithClientIp()
            .Enrich.WithCorrelationId()
            .Enrich.WithEnvironmentUserName()
            .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
            .Enrich.WithProperty("Environment", environment);

        var contextLogger = ContextLogger.WithMachineName(loggerConfiguration.Enrich);
        var environmentLogger = EnvironmentLogger.WithMachineName(contextLogger.Enrich);

        Log.Logger = environmentLogger.CreateLogger();
        return Log.Logger;
    }
}