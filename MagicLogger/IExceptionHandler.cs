namespace MagicLogger;

public interface IExceptionHandler
{
    Task HandleException(HttpContext context, Exception ex);
}
