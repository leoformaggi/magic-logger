using MagicLogger;

public class MyErrorHandler : IExceptionHandler
{
    public async Task HandleException(HttpContext context, Exception ex)
    {
        //do something custom
        if (ex is TimeoutException)
            context.Response.StatusCode = 123;
    }
}
