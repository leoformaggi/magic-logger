using MagicLogger.Helpers;
using System.Diagnostics;

namespace MagicLogger.Middlewares.Internal;

internal class TimeElapsedMiddleware
{
    private readonly RequestDelegate _next;

    public TimeElapsedMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await _next(context);

        sw.Stop();

        context.Items[Constants.TimeElapsedHeaderName] = sw.ElapsedMilliseconds.ToString();
    }
}