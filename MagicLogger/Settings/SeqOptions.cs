#nullable disable
using Serilog.Events;

namespace MagicLogger.Settings;

public class SeqOptions
{
    public bool Enabled { get; set; }

    public LogEventLevel? MinimumLevel { get; set; }

    public string Url { get; set; }

    public string ApiKey { get; set; }
}
