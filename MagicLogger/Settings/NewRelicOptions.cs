#nullable disable
using Serilog.Events;

namespace MagicLogger.Settings;

public class NewRelicOptions
{
    public bool Enabled { get; set; }

    public LogEventLevel? MinimumLevel { get; set; }

    public string AppName { get; set; }

    public string LicenseKey { get; set; }
}
