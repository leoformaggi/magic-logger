using MagicLogger.Settings;

namespace MagicLogger.Middlewares.Internal;

internal class BufferRequestMiddleware
{
    private readonly RequestDelegate _next;

    public BufferRequestMiddleware(RequestDelegate next, LogSettings logSettings)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        await _next(context);
    }
}