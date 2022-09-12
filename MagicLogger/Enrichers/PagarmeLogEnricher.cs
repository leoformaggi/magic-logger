using MagicLogger.Data;
using MagicLogger.Helpers;
using MagicLogger.Settings;
using Serilog.Core;
using Serilog.Events;
using System.Net;
using Constants = MagicLogger.Helpers.Constants;

namespace MagicLogger.Enrichers;

internal class PagarmeLogEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LogSettings _logSettings;

    public PagarmeLogEnricher() : this(new HttpContextAccessor(), new LogSettings())
    { }

    public PagarmeLogEnricher(IHttpContextAccessor httpContextAccessor, LogSettings logSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _logSettings = logSettings;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = _httpContextAccessor?.HttpContext;

        if (context is not null)
            HandleLog(logEvent, propertyFactory, context);
    }

    private void HandleLog(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, HttpContext context)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sourceContext))
            if (sourceContext.ToString() is "\"MagicLogger.Middlewares.Internal.LoggerMiddleware\"")
                AddDefaultProperties(logEvent, propertyFactory, context);

        RemoveSerilogDefaultProperties(logEvent);

        var addInfoTransient = context.RequestServices.GetRequiredService<TransientAdditionalInfoLogger>();
        foreach (var item in addInfoTransient.GetData())
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(item.Key, item.Value));

        addInfoTransient.ClearData();
    }

    private static void RemoveSerilogDefaultProperties(LogEvent logEvent)
    {
        logEvent.RemovePropertyIfPresent("ConnectionId");
        logEvent.RemovePropertyIfPresent("RequestId");
        logEvent.RemovePropertyIfPresent("SourceContext");
        logEvent.RemovePropertyIfPresent("RequestPath");
    }

    private void AddDefaultProperties(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, HttpContext context)
    {
        var exception = (Exception)context.Items["exception"]!;

        string? exceptionMsg = null, stackTrace = null;
        if (exception != null)
        {
            exceptionMsg = HandleFieldSize(exception.Message, 256);
            stackTrace = HandleFieldSize(exception.StackTrace, 1024);
        }

        context.Items.TryGetValue("Controller", out object? controller);
        context.Items.TryGetValue("Action", out object? action);
        context.Items.TryGetValue("Version", out object? version);

        int statusCode = context.GetStatusCode(exception);
        var httpStatusCode = (HttpStatusCode)statusCode;

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestBody", context.GetRequestBody(_logSettings.JsonBlacklistRequest)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ResponseContent", context.GetResponseContent(_logSettings.JsonBlacklistResponse)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Method", context.Request.Method));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Path", context.Request.Path));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Host", context.GetHost()));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Port", context.GetPort()));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Url", context.GetFullUrl(_logSettings.QueryStringBlacklist)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("QueryString", context.GetRawQueryString(_logSettings.QueryStringBlacklist)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Query", context.GetQueryString(_logSettings.QueryStringBlacklist)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestHeaders", context.GetRequestHeaders(_logSettings.HeaderBlacklist)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Ip", context.GetIp()));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("User", context.GetUser()));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("IsSuccessful", statusCode < 400));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("StatusCode", statusCode));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("StatusDescription", httpStatusCode.ToString()));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("StatusCodeFamily", context.GetStatusCodeFamily(exception)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ProtocolVersion", context.Request.Protocol));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ErrorException", stackTrace));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ErrorMessage", exceptionMsg));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ContentType", context.Response.ContentType));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ResponseHeaders", context.GetResponseHeaders(_logSettings.HeaderBlacklist)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ElapsedMilliseconds", context.GetExecutionTime(Constants.TimeElapsedHeaderName)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestKey", context.GetRequestKey(Constants.RequestKeyHeaderName)));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ContentLength", context.GetResponseLength()));

        var addInfoScoped = context.RequestServices.GetRequiredService<ScopedAdditionalInfoLogger>();

        foreach (var item in addInfoScoped.GetData())
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(item.Key, item.Value));
    }

    private static string? HandleFieldSize(string? value, int maxSize, bool required = false, string defaultValue = "????")
    {
        if (string.IsNullOrWhiteSpace(value) && !required)
            return null;

        if (string.IsNullOrWhiteSpace(value))
            value = defaultValue;

        if (value.Length > maxSize)
            return value[..maxSize];

        return value;
    }
}