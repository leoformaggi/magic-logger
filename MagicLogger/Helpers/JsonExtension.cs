using Newtonsoft.Json.Linq;

namespace MagicLogger.Helpers;

internal static class JsonExtension
{
    public static object? DeserializeAsObject(this string json)
    {
        return DeserializeAsObjectCore(JToken.Parse(json));
    }

    public static object? DeserializeAsObjectCore(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                return token.Children<JProperty>().ToDictionary((prop) => prop.Name, (prop) => DeserializeAsObjectCore(prop.Value));
            case JTokenType.Array:
                return token.Select(DeserializeAsObjectCore).ToList();
            default:
                return ((JValue)token).Value;
        }
    }
}