using MagicLogger.Helpers;

namespace MagicLogger.Middlewares.Internal;
/*
    Oferecer uma classe de RequestKey com Value para os clients injetarem e nesse código ela é associada?
*/
internal class RequestKeyMiddleware
{
    private readonly RequestDelegate _next;

    public RequestKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string rk;
        if (context.Request.Headers.ContainsKey(Constants.RequestKeyHeaderName))
            rk = context.Request.Headers[Constants.RequestKeyHeaderName];
        else
            rk = Guid.NewGuid().ToString();

        context.Items.Add(Constants.RequestKeyHeaderName, rk);
        context.Response.Headers.Add(Constants.RequestKeyHeaderName, rk);

        await _next(context);
    }
}

public class Requestkey
{
    public string Value { get; set; }
}
