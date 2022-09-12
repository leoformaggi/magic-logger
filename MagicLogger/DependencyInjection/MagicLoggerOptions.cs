using MagicLogger.Settings;
using Serilog.Events;

namespace MagicLogger.DependencyInjection;

public class MagicLoggerOptions
{
    private IConfigurationBuilder _configurationBuilder;
    private IConfiguration? _providedConfiguration;

    private string? _envPrefix;
    private string _logSettingsSectionName = nameof(Settings.LogSettings);
    private SeqOptions? _seqOptions;
    private SplunkOptions? _splunkOptions;
    private NewRelicOptions? _newRelicOptions;

    internal LogSettings? LogSettings { get; private set; }
    internal LogEventLevel MinimumLogEventLevel { get; set; } = LogEventLevel.Information;

    internal MagicLoggerOptions()
    {
        _configurationBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory());
    }

    public void UseEnvironmentVariablePrefix(string prefix)
    {
        _envPrefix = prefix;
    }

    public void SetLogSettingsSectionName(string sectionName)
    {
        _logSettingsSectionName = sectionName;
    }

    public void SetMinimumLogEventLevel(LogEventLevel logEventLevel)
    {
        MinimumLogEventLevel = logEventLevel;
    }

    public void UseOptionsFromConfiguration(IConfiguration configuration)
    {
        _providedConfiguration = configuration;
    }

    public void AddOptionsFromJsonFile(string fileName)
    {
        _configurationBuilder.AddJsonFile(fileName, optional: true, reloadOnChange: true);
    }

    public void AddOptionsFromEnvironment()
    {
        if (_envPrefix is null)
            _configurationBuilder.AddEnvironmentVariables();
        else
            _configurationBuilder.AddEnvironmentVariables(_envPrefix);
    }

    public void UseLogSettings(LogSettings logSettings)
    {
        LogSettings = logSettings;
    }

    public void UseSeq(SeqOptions options)
    {
        _seqOptions = options;
    }

    public void UseSplunk(SplunkOptions options)
    {
        _splunkOptions = options;
    }

    public void UseNewRelic(NewRelicOptions options)
    {
        _newRelicOptions = options;
    }

    internal void OverrideSinksOptions()
    {
        ConstructLogSettings();

        if (_seqOptions is not null)
            LogSettings!.SeqOptions = _seqOptions;

        if (_newRelicOptions is not null)
            LogSettings!.NewRelicOptions = _newRelicOptions;

        if (_splunkOptions is not null)
            LogSettings!.SplunkOptions = _splunkOptions;
    }

    private void ConstructLogSettings()
    {
        if (LogSettings is not null)
            return;

        var configuration = _providedConfiguration ?? _configurationBuilder.Build();

        LogSettings = configuration.GetSection(_logSettingsSectionName).Get<LogSettings>();

        if (LogSettings is null)
            throw new Exception("Could not load LogSettings. Please provide appropriate configurations.");
    }
}