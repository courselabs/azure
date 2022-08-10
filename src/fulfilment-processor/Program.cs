using Fulfilment.Processor.Configuration;
using Fulfilment.Processor.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Templates;
using System;
using System.Threading;
using Timers = System.Timers;

namespace Fulfilment.Processor
{
    class Program
    {
        private static CancellationTokenSource _Cancellation = new CancellationTokenSource();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                _Cancellation.Cancel();
            };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json")
                         .AddEnvironmentVariables()
                         .AddJsonFile("config/override.json", optional: true, reloadOnChange: true);
            var config = configBuilder.Build();

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddOptions()
                    .Configure<ObservabilityOptions>(config.GetSection("Observability"))
                .AddTransient<DocumentProcessor>();

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>().Value;

            // configure Serilog
            Log.Logger = CreateLogger(config, options.Logging);
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger));
            serviceProvider = services.BuildServiceProvider();

            if (options.ExitAfterSeconds > 0)
            {
                var expiration = TimeSpan.FromSeconds(options.ExitAfterSeconds);
                var exitTimer = new Timers.Timer(expiration.TotalMilliseconds)
                {
                    AutoReset = false,
                    Enabled = true
                };
                exitTimer.Elapsed += (s, args) =>
                {
                    Log.Fatal("{EventType}: Out of memory! Exiting immediately. Goodbye.", "EXIT");
                    Log.CloseAndFlush();
                    _Cancellation.Cancel();
                };
            }

            if (options.StartupDelaySeconds > 0)
            {
                _Cancellation.Token.WaitHandle.WaitOne(options.StartupDelaySeconds * 1000);
            }

            Log.Information("{EventType}: Processor starting", "STARTUP");
            var processor = serviceProvider.GetRequiredService<DocumentProcessor>();
            processor.GenerateRandom(_Cancellation);
        }

        private static Serilog.Core.Logger CreateLogger(IConfiguration config, LoggingOptions loggingOptions)
        {
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            
            loggerConfig.Enrich.WithAppVersion()
                        .Enrich.WithAppName();

            return loggerConfig.CreateLogger();
        }
    }
}