namespace MagicLogger.Middlewares.Internal;

internal class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IExceptionHandler _exceptionHandler;

    public ExceptionHandlerMiddleware(RequestDelegate next, IExceptionHandler exceptionHandler)
    {
        _next = next;
        _exceptionHandler = exceptionHandler;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            context.Items.Add("exception", e);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await _exceptionHandler.HandleException(context, e);
        }
    }
}