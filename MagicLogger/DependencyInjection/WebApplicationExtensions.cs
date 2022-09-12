namespace MagicLogger.DependencyInjection;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseMagicLoggerMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<LoggerMiddleware>();
        app.UseMiddleware<RequestKeyMiddleware>();
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseMiddleware<BufferRequestMiddleware>();
        app.UseMiddleware<TimeElapsedMiddleware>();

        return app;
    }
}