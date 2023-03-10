using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using Serilog.Core;

namespace Maestro
{
    internal static class Logger
    {
        internal static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
            .Enrich.With(new ThreadIdEnricher())
            .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} ({ThreadId}) [{Level:u3}] {Message}{NewLine}{Exception}",
                             theme: SystemConsoleTheme.Colored)
            .WriteTo.File(Path.Combine("logs", "maestro.log"),
                          LogEventLevel.Debug, // Changing this will filter the log verbosity
                          outputTemplate: "{Timestamp:ddMMMyyyy_HH:mm:ss.fff} ({ThreadId}) [{Level:u3}] {Message}{NewLine}{Exception}",
                          rollingInterval: RollingInterval.Day, // New log every day, can also split by filesize if needed
                          shared: true)
            .MinimumLevel.Verbose() // Need this line to enable any level of verbosity beyond `Information`
            .CreateLogger();
        }
    }

    /// <summary>
    /// Serilog specific class that includes the thread ID of the logging instance
    /// </summary>
    internal class ThreadIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ThreadId", Environment.CurrentManagedThreadId));
        }
    }
}
