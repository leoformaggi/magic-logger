using MagicLogger.Data;
using MagicLogger.Enrichers;
using MagicLogger.Formatters;
using MagicLogger.Settings;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace MagicLogger.DependencyInjection;

public static class WebApplicationBuilderExtensions
{
    public static void SetupMagicLogger(this WebApplicationBuilder builder, Action<MagicLoggerOptions> options)
    {
        var opts = new MagicLoggerOptions();
        options(opts);
        opts.OverrideSinksOptions();

        builder.Logging.ClearProviders();

        AddServices(builder, opts.LogSettings!);

        builder.Host.UseSerilog((ctx, svcProv, config) => ConfigureLogger(ctx, svcProv, config, opts));
    }

    public static void CustomizeMagicLoggerMiddlewares(this IServiceCollection serviceCollection, Action<MagicLoggerMiddlewareOptions> options)
    {
        var opt = new MagicLoggerMiddlewareOptions();
        options(opt);

        serviceCollection.AddTransient(typeof(IExceptionHandler), opt.ExceptionHandlerType);
    }

    private static void AddServices(WebApplicationBuilder builder, LogSettings logSettings)
    {
        builder.Services.AddScoped<ScopedAdditionalInfoLogger>();
        builder.Services.AddScoped<TransientAdditionalInfoLogger>();

        builder.Services.AddSingleton(logSettings);

        builder.Services.AddTransient<ILogEventEnricher, PagarmeLogEnricher>();

        builder.Services.AddHttpContextAccessor();
    }

    private static void ConfigureLogger(HostBuilderContext context, IServiceProvider serviceProvider, LoggerConfiguration configuration, MagicLoggerOptions options)
    {
        configuration
        .Enrich.FromLogContext()
        .ReadFrom.Services(serviceProvider)
        .MinimumLevel.Is(options.MinimumLogEventLevel)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information);

        AddSinks(configuration, options);
    }

    private static void AddSinks(LoggerConfiguration configuration, MagicLoggerOptions options)
    {
        AddSplunk(configuration, options);
        AddSeq(configuration, options);
        AddNewRelic(configuration, options);
    }

    private static void AddNewRelic(LoggerConfiguration configuration, MagicLoggerOptions options)
    {
        if (options.LogSettings!.NewRelicOptions is { Enabled: true } newRelic)
        {
            if (string.IsNullOrWhiteSpace(newRelic.LicenseKey))
                newRelic.LicenseKey = Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY");

            ArgumentNullException.ThrowIfNull(newRelic.LicenseKey);

            configuration.WriteTo.NewRelicLogs("https://log-api.newrelic.com/log/v1", newRelic.AppName,
                newRelic.LicenseKey, null, restrictedToMinimumLevel: newRelic.MinimumLevel ?? options.MinimumLogEventLevel);
        }
    }

    private static void AddSeq(LoggerConfiguration configuration, MagicLoggerOptions options)
    {
        if (options.LogSettings!.SeqOptions is { Enabled: true } seq)
        {
            ArgumentNullException.ThrowIfNull(seq.Url);

            configuration.WriteTo.Seq(seq.Url, restrictedToMinimumLevel: seq.MinimumLevel ?? options.MinimumLogEventLevel, apiKey: seq.ApiKey);
        }
    }

    private static void AddSplunk(LoggerConfiguration configuration, MagicLoggerOptions options)
    {
        if (options.LogSettings!.SplunkOptions is { Enabled: true } splunk)
        {
            ArgumentNullException.ThrowIfNull(splunk.Url);

            configuration.WriteTo.EventCollector(splunk.Url, splunk.Token,
               jsonFormatter: new PagarmeSplunkJsonFormatter(splunk),
               restrictedToMinimumLevel: splunk.MinimumLevel ?? options.MinimumLogEventLevel);
        }
    }
}