namespace MagicLogger;

internal class DefaultExceptionHandler : IExceptionHandler
{
    public Task HandleException(HttpContext context, Exception ex)
    {
        // do nothing as default
        return Task.CompletedTask;
    }
}
