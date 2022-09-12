using MagicLogger.Settings;

namespace MagicLogger.Middlewares.Internal;

internal class LoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggerMiddleware> _logger;
    private const string DEFAULT_LOG_MESSAGE = "HTTP {Method} {Path} from {Ip} responded {StatusCode} in {ElapsedMilliseconds} ms";
    private readonly string _logMessage;

    public LoggerMiddleware(RequestDelegate next, ILogger<LoggerMiddleware> logger, LogSettings settings)
    {
        _next = next;
        _logger = logger;
        _logMessage = string.IsNullOrWhiteSpace(settings.TitlePrefix) ? DEFAULT_LOG_MESSAGE : settings.TitlePrefix + DEFAULT_LOG_MESSAGE;
    }

    public async Task Invoke(HttpContext context)
    {
        using var ms = new MemoryStream();
        var stream = context.Response.Body;
        context.Response.Body = ms;

        await _next(context);

        ms.Position = 0;

        if (context.Items.ContainsKey("exception"))
            _logger.LogError(_logMessage);
        else
            _logger.LogInformation(_logMessage);

        ms.Position = 0;
        await ms.CopyToAsync(stream);
        context.Response.Body = stream;
    }
}