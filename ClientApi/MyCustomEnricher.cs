using Serilog.Core;
using Serilog.Events;

public class MyCustomEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MyCustomEnricher() : this(new HttpContextAccessor())
    { }

    public MyCustomEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = _httpContextAccessor?.HttpContext;

        if (context is not null)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MyCustomProperty", context.Items["KeyFromSomeCustomMiddleware"]));
    }
}