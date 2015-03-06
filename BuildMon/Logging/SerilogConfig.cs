using System;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.RollingFile;

namespace BuildMon.Logging
{
    public class SerilogConfig
    {
        public static ILogger CreateLogger()
        {
            var loggerFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BuildMon";

            var config = new LoggerConfiguration().
                MinimumLevel.Debug().
                WriteTo.Sink(
                    new RollingFileSink(loggerFolder + "\\serilog-{Date}.json", new JsonFormatter(renderMessage: true),
                        1024*1024*100, 7), LogEventLevel.Debug).
                WriteTo.Sink(
                    new RollingFileSink(loggerFolder + "\\serilog-{Date}-norender.json",
                        new JsonFormatter(renderMessage: false), 1024*1024*100, 7), LogEventLevel.Debug).
                WriteTo.RollingFile(loggerFolder + "\\serilog-debug-{Date}.log", LogEventLevel.Debug,
                    fileSizeLimitBytes: 1024*1024*100, retainedFileCountLimit: 7);

            InitialiseGlobalContext(config);

            return config.CreateLogger();
        }

        public static LoggerConfiguration InitialiseGlobalContext(LoggerConfiguration configuration)
        {
            return configuration.Enrich.WithMachineName()
                .Enrich.WithProperty("ApplicationName", typeof (SerilogConfig).Assembly.GetName().Name)
                .Enrich.WithProperty("UserName", Environment.UserName)
                .Enrich.WithProperty("AppDomain", AppDomain.CurrentDomain)
                .Enrich.WithProperty("RuntimeVersion", Environment.Version)
                // this ensures that calls to LogContext.PushProperty will cause the logger to be enriched
                .Enrich.FromLogContext();
        }
    }
}