namespace MagicLogger.DependencyInjection.Options;

public class MagicLoggerMiddlewareOptions
{
    internal Type ExceptionHandlerType { get; private set; } = typeof(DefaultExceptionHandler);

    public void UseCustomExceptionHandler<T>() where T : IExceptionHandler
    {
        ExceptionHandlerType = typeof(T);
    }
}
