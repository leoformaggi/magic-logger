using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Web;
using System.Xml.Linq;

namespace MagicLogger.Helpers;

internal static class HttpContextExtractor
{
    public static int GetStatusCode(this HttpContext context, Exception? exception)
    {
        if (exception is not null)
            return 500;

        return (context?.Response?.StatusCode).GetValueOrDefault();
    }

    public static string GetStatusCodeFamily(this HttpContext context, Exception? exception)
    {
        return context.GetStatusCode(exception).ToString()[0] + "XX";
    }

    public static IDictionary<string, string>? GetQueryString(this HttpContext context, string[]? blacklist)
    {
        if (context?.Request?.Query is null)
            return null;

        var dictionary = new Dictionary<string, string>();
        foreach (var item in context.Request.Query)
            dictionary[item.Key] = MaskField(item.Key, item.Value.ToString(), blacklist);

        return dictionary;
    }

    public static string GetRawQueryString(this HttpContext context, string[]? blacklist)
    {
        if (context?.Request?.Query is null)
            return string.Empty;

        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(context.Request.QueryString.ToString());
        string[] allKeys = nameValueCollection.AllKeys!;
        foreach (string text in allKeys)
            nameValueCollection[text] = MaskField(text, nameValueCollection[text]!, blacklist);

        return $"?{nameValueCollection}";
    }

    public static IDictionary<string, string>? GetRequestHeaders(this HttpContext context, string[]? blacklist)
    {
        if (context?.Request?.Headers is null)
            return null;

        var dictionary = new Dictionary<string, string>();
        foreach (var header in context.Request.Headers)
            dictionary[header.Key] = MaskField(header.Key, header.Value.ToString(), blacklist);

        return dictionary;
    }

    public static IDictionary<string, string>? GetResponseHeaders(this HttpContext context, string[]? blacklist)
    {
        if (context?.Response?.Headers is null)
            return null;

        var dictionary = new Dictionary<string, string>();
        foreach (KeyValuePair<string, StringValues> header in context.Response.Headers)
            dictionary[header.Key] = MaskField(header.Key, header.Value.ToString(), blacklist);

        return dictionary;
    }

    public static object GetExecutionTime(this HttpContext context, string timeElapsedProperty)
    {
        long num = -1L;
        object value = "-1";
        if (context?.Items?.TryGetValue(timeElapsedProperty, out value!) == true && long.TryParse(value!.ToString(), out long result))
            return result;

        return num;
    }

    public static string? GetRequestKey(this HttpContext context, string requestKeyProperty)
    {
        if (string.IsNullOrWhiteSpace(requestKeyProperty))
            return null;

        if (context is not null && context.Items?.ContainsKey(requestKeyProperty) == true)
            return context.Items[requestKeyProperty]!.ToString();

        return null;
    }

    public static string GetIp(this HttpContext context)
    {
        string text = "??";
        if (context?.Request?.Headers is null)
            return text;

        if (context.Request.Headers.Any(r => r.Key == "X-Forwarded-For"))
            return context.Request.Headers["X-Forwarded-For"].First();

        return context.Connection?.RemoteIpAddress?.ToString() ?? text;
    }

    public static string? GetUser(this HttpContext context)
    {
        return context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    }

    public static object? GetRequestBody(this HttpContext context, string[]? blacklist)
    {
        if (context?.Request?.Body is null)
            return null;

        var syncIOFeature = context.Features.Get<IHttpBodyControlFeature>();
        if (syncIOFeature is not null)
            syncIOFeature.AllowSynchronousIO = true;

        try
        {
            context.Request.Body.Position = 0L;
        }
        catch { }

        string? text = null;
        using (StreamReader streamReader = new StreamReader(context.Request.Body, leaveOpen: true))
            text = streamReader.ReadToEnd();

        string obj = context.Request.Headers.ContainsKey("Content-Type")
            ? string.Join(";", context.Request.Headers["Content-Type"])
            : string.Empty;

        if (obj.Contains("json"))
            return GetContentAsObjectByContentTypeJson(text, maskJson: true, blacklist);

        if (obj.Contains("xml"))
            return GetContentAsObjectByContentTypeXml(text, maskXml: true, blacklist);

        return new Dictionary<string, string>
            {
                {
                    "raw_body",
                    text
                }
            };
    }

    public static object? GetHost(this HttpContext context)
    {
        return context?.Request?.Host.ToString().Split(':').FirstOrDefault();
    }

    public static object? GetPort(this HttpContext context)
    {
        string[] source = context?.Request?.Host.ToString().Split(':')!;

        if (source.Count() > 1)
            return source.LastOrDefault();

        if (context?.Request?.Protocol == "http")
            return 80;

        if (context?.Request?.Protocol == "https")
            return 443;

        return 0;
    }

    public static object? GetFullUrl(this HttpContext context, string[]? blacklist)
    {
        if (context?.Request is null)
            return null;

        return context.Request.Scheme + "://" +
            context.Request.Host.ToUriComponent() +
            context.Request.PathBase.ToUriComponent() +
            context.Request.Path.ToUriComponent() +
            context.GetRawQueryString(blacklist);
    }

    public static object? GetResponseContent(this HttpContext context, string[]? blacklist)
    {
        if (context?.Response is not null && context.Response.Body?.CanRead == false)
            return null;

        MemoryStream memoryStream = new MemoryStream();
        context!.Response.Body!.Seek(0L, SeekOrigin.Begin);
        context.Response.Body.CopyTo(memoryStream);
        context.Response.Body.Seek(0L, SeekOrigin.Begin);
        memoryStream.Seek(0L, SeekOrigin.Begin);

        string? text = null;
        using (StreamReader streamReader = new StreamReader(memoryStream))
            text = streamReader.ReadToEnd();

        if (!string.IsNullOrWhiteSpace(text) && context.Response.ContentType.Contains("json"))
            return GetContentAsObjectByContentTypeJson(text, maskJson: true, blacklist);

        if (!string.IsNullOrWhiteSpace(text) && context.Response.ContentType.Contains("xml"))
            return GetContentAsObjectByContentTypeXml(text, maskXml: true, blacklist);

        return new Dictionary<string, string>
            {
                {
                    "raw_content",
                    text
                }
            };
    }

    public static long GetResponseLength(this HttpContext context)
    {
        try
        {
            return (context?.Response?.Body?.Length).GetValueOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    internal static object? GetContentAsObjectByContentTypeJson(string content, bool maskJson, string[]? backlist)
    {
        try
        {
            if (maskJson && backlist is not null && backlist.Any())
                content = content.MaskFields(backlist, "******");

            return content.DeserializeAsObject();
        }
        catch (Exception)
        {
            return content;
        }
    }

    internal static object? GetContentAsObjectByContentTypeXml(string content, bool maskXml, string[]? blacklist)
    {
        string content2 = null;
        using (StringReader stringReader = new StringReader(content))
            content2 = JsonConvert.SerializeXNode(XDocument.Parse(stringReader.ReadToEnd()));

        return GetContentAsObjectByContentTypeJson(content2!, maskXml, blacklist);
    }

    internal static string MaskField(string key, string value, string[]? blacklist)
    {
        if (blacklist is not null && blacklist.Any() && blacklist.Contains(key))
            return "******";

        return value;
    }
}
