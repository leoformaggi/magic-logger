namespace MagicLogger.Settings;

public partial class LogSettings
{
    public string? TitlePrefix { get; set; }

    public string[]? JsonBlacklistRequest { get; set; }

    public string[]? JsonBlacklistResponse { get; set; }

    public string[]? HeaderBlacklist { get; set; }

    public string[]? QueryStringBlacklist { get; set; }

    public bool DebugEnabled { get; set; }

    public SeqOptions SeqOptions { get; set; } = new SeqOptions();

    public SplunkOptions SplunkOptions { get; set; } = new SplunkOptions();

    public NewRelicOptions NewRelicOptions { get; set; } = new NewRelicOptions();
}