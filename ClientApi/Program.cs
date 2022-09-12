using MagicLogger.DependencyInjection;
using Serilog.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// You can add Serilog Enrichers if you want. Magic Logger will read from the ServiceCollection and use them all
builder.Services.AddTransient<ILogEventEnricher, MyCustomEnricher>();

builder.Services.CustomizeMagicLoggerMiddlewares(opt =>
{
    opt.UseCustomExceptionHandler<MyErrorHandler>();
});

builder.SetupMagicLogger(opt =>
{
    // Add LogSettings from json file
    opt.AddOptionsFromJsonFile("appsettings.json");

    // Add LogSettings from Env Vars
    opt.UseEnvironmentVariablePrefix("ENV_");
    opt.AddOptionsFromEnvironment();

    // Change IConfiguration section name to search for when building configuration
    //opt.SetLogSettingsSectionName("SomeLogSectionName");

    // Customize minimum event level to log for all sinks (each sink can override as it wants)
    if (builder.Environment.IsDevelopment())
        opt.SetMinimumLogEventLevel(Serilog.Events.LogEventLevel.Debug);
    else
        opt.SetMinimumLogEventLevel(Serilog.Events.LogEventLevel.Information);

    // Provide own IConfiguration if wanted/needed.
    // The Setup will use the provided IConfiguration and ignore the above lines
    //var myConfig = new ConfigurationBuilder().AddInMemoryCollection().Build();
    //opt.UseOptionsFromConfiguration(myConfig);

    // It's also possible to provide objects for all LogSettings
    //opt.UseLogSettings(new MagicLogger.Settings.LogSettings());
    //opt.UseSeq(new MagicLogger.Settings.SeqOptions());
    //opt.UseSplunk(new MagicLogger.Settings.SplunkOptions());
    //opt.UseNewRelic(new MagicLogger.Settings.NewRelicOptions());
});

var app = builder.Build();

app.MapGet("/weatherforecast", Endpoints.GetWeather).WithName("GetWeatherForecast");

// Adding other middlewares before the Magic Logger's one won't have anything logged
app.UseMagicLoggerMiddleware();
// Adding other middlewares after the Magic Logger's one may potentially be logger if you want,
// but it needs some setting.

app.UseSwagger();
app.UseSwaggerUI();

app.Run();