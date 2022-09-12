#nullable disable
using Serilog.Events;
using Serilog.Formatting;

namespace MagicLogger.Settings;

public class SplunkOptions
{
    public bool Enabled { get; set; }

    public LogEventLevel? MinimumLevel { get; set; }

    public string Index { get; set; }

    public string Application { get; set; }

    public string ProcessName { get; set; }

    public string Company { get; set; }

    public string ProductVersion { get; set; }

    public string Url { get; set; }

    public string SourceType { get; set; }

    public string Token { get; set; }

    public ITextFormatter TextFormatter { get; set; }
}
