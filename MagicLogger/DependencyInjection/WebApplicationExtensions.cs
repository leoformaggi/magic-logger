namespace MagicLogger.DependencyInjection;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseMagicLoggerMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<BufferRequestMiddleware>();
        app.UseMiddleware<LoggerMiddleware>();
        app.UseMiddleware<RequestKeyMiddleware>();
        app.UseMiddleware<TimeElapsedMiddleware>();
        app.UseMiddleware<ExceptionHandlerMiddleware>();

        return app;
    }
}