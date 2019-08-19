using System;
using System.ComponentModel;
using System.Text;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Formatting.Json;
using Serilog.Sinks.NamedPipe;
using Serilog.Sinks.SystemConsole.Themes;

namespace Serilog
{
    /// <summary>Extends <see cref="LoggerConfiguration"/> with methods to add pipe sinks.</summary>
    public static class PipeLoggerExtensions
    {
        /// <summary>
        /// Writes log events to <see cref="System.IO.Pipes.NamedPipeServerStream"/>.
        /// </summary>
        /// <param name="sinkConfiguration">Logger sink configuration.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="outputTemplate">A message template describing the format used to write to the sink.
        /// The default is <code>"[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"</code>.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level
        /// to be changed at runtime.</param>
        /// <param name="theme">The theme to apply to the styled output. If not specified,
        /// uses <see cref="SystemConsoleTheme.Literate"/>.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration NamedPipe(
            this LoggerSinkConfiguration sinkConfiguration,
            string pipeName,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            Encoding encoding = null
        )
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (pipeName == null) throw new ArgumentNullException(nameof(pipeName));
            
            return sinkConfiguration.Sink(new NamedPipeSink(pipeName, encoding), restrictedToMinimumLevel, levelSwitch);
        }

        public static ILogger UseNamedPipeClient(
            this ILogger logger,
            string pipeName,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            Encoding encoding = null
        )
        {
            if (pipeName == null) throw new ArgumentNullException(nameof(pipeName));
            logger = logger.ForContext(new ClientEnricher());
            var c = new NamedPipeClient(pipeName, logger);
            return logger;
        }
    }
}
