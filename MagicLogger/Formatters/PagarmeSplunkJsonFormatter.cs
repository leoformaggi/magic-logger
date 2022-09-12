using MagicLogger.Settings;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace MagicLogger.Formatters;

internal class PagarmeSplunkJsonFormatter : ITextFormatter
{
    private static readonly JsonValueFormatter ValueFormatter = new JsonValueFormatter("$type");
    private string _suffix = default!;
    private readonly SplunkOptions _splunkSettings;

    public PagarmeSplunkJsonFormatter(SplunkOptions splunkSettings)
    {
        _splunkSettings = splunkSettings;
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(output);

        FillLogSplunkStone(logEvent, output);
    }

    private void FillLogSplunkStone(LogEvent logEvent, TextWriter output)
    {
        DefaultSuffixPropertiesSplunk();
        FormatPropertiesSplunk(logEvent, output);
    }

    private void DefaultSuffixPropertiesSplunk()
    {
        StringWriter stringWriter = new StringWriter();
        stringWriter.Write("}");
        if (!string.IsNullOrWhiteSpace(_splunkSettings.Application))
        {
            stringWriter.Write(",\"source\":");
            JsonValueFormatter.WriteQuotedJsonString(_splunkSettings.Application, stringWriter);
        }

        if (!string.IsNullOrWhiteSpace(_splunkSettings.SourceType))
        {
            stringWriter.Write(",\"sourcetype\":");
            JsonValueFormatter.WriteQuotedJsonString(_splunkSettings.SourceType, stringWriter);
        }

        if (!string.IsNullOrWhiteSpace(Environment.MachineName))
        {
            stringWriter.Write(",\"host\":");
            JsonValueFormatter.WriteQuotedJsonString(Environment.MachineName, stringWriter);
        }

        if (!string.IsNullOrWhiteSpace(_splunkSettings.Index))
        {
            stringWriter.Write(",\"index\":");
            JsonValueFormatter.WriteQuotedJsonString(_splunkSettings.Index, stringWriter);
        }

        stringWriter.Write('}');
        _suffix = stringWriter.ToString();
    }

    private void FormatPropertiesSplunk(LogEvent logEvent, TextWriter output)
    {
        output.Write("{\"time\":\"");
        output.Write((long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        output.Write("\",\"event\":{");

        string msg = logEvent.RenderMessage().Replace("\"", "") ?? _splunkSettings.ProcessName;
        if (!string.IsNullOrWhiteSpace(msg))
        {
            output.Write("\"Message\":");
            JsonValueFormatter.WriteQuotedJsonString(msg, output);
        }

        if (!string.IsNullOrWhiteSpace(_splunkSettings.ProcessName))
        {
            output.Write(",\"ProcessName\":");
            JsonValueFormatter.WriteQuotedJsonString(_splunkSettings.ProcessName, output);
        }

        if (!string.IsNullOrWhiteSpace(_splunkSettings.Company))
        {
            output.Write(",\"ProductCompany\":");
            JsonValueFormatter.WriteQuotedJsonString(_splunkSettings.Company, output);
        }

        if (!string.IsNullOrWhiteSpace(_splunkSettings.Application))
        {
            output.Write(",\"ProductName\":");
            JsonValueFormatter.WriteQuotedJsonString(_splunkSettings.Application, output);
        }

        if (!string.IsNullOrWhiteSpace(_splunkSettings.ProductVersion))
        {
            output.Write(",\"ProductVersion\":");
            JsonValueFormatter.WriteQuotedJsonString(_splunkSettings.ProductVersion, output);
        }

        string severity = logEvent.Level.ToString();
        if (!string.IsNullOrWhiteSpace(severity))
        {
            output.Write(",\"Severity\":");
            JsonValueFormatter.WriteQuotedJsonString(severity, output);
        }

        string[] aditionalDataToBeRemoved = new string[4]
        {
                "SplunkIndex",
                "ProductCompany",
                "ProductVersion",
                "ProcessName"
        };
        var properties = logEvent.Properties.Where(p => !aditionalDataToBeRemoved.Contains(p.Key)).ToDictionary(d => d.Key, d => d.Value);

        WriteProperties(properties, output, logEvent.Exception);
        output.WriteLine(_suffix);
    }

    private void WriteProperties(Dictionary<string, LogEventPropertyValue> properties, TextWriter output, Exception exception)
    {
        output.Write(",\"AdditionalData\":{");
        string text = "";
        if (properties.Count != 0)
        {
            foreach (var property in properties)
            {
                output.Write(text);
                text = ",";
                JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                output.Write(':');
                ValueFormatter.Format(property.Value, output);
            }
        }

        WriteException(exception, text, output);
        output.Write('}');
    }

    private void WriteException(Exception exception, string precedingDelimiter, TextWriter output)
    {
        if (exception is not null)
        {
            output.Write(precedingDelimiter);
            output.Write("\"Exception\":{");
            JsonValueFormatter.WriteQuotedJsonString("Message", output);
            output.Write(':');
            JsonValueFormatter.WriteQuotedJsonString(exception.Message, output);
            output.Write(",");
            JsonValueFormatter.WriteQuotedJsonString("StackTrace", output);
            output.Write(':');
            JsonValueFormatter.WriteQuotedJsonString(exception.StackTrace, output);
            output.Write('}');
        }
    }
}
