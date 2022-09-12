using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace MagicLogger.Helpers;

internal static class JsonMasking
{
    public static string MaskFields(this string json, string[] blacklist, string mask)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentNullException("json");

        ArgumentNullException.ThrowIfNull(blacklist);

        if (!blacklist.Any())
            return json;

        JObject obj = (JObject)JsonConvert.DeserializeObject(json)!;
        MaskFieldsFromJToken(obj, blacklist, mask);
        return obj.ToString();
    }

    private static void MaskFieldsFromJToken(JToken token, string[] blacklist, string mask)
    {
        JContainer jContainer = token as JContainer;
        if (jContainer is null)
            return;

        List<JToken> list = new List<JToken>();
        foreach (JToken item in jContainer.Children())
        {
            JProperty prop = item as JProperty;
            if (prop != null && blacklist.Any((item) => Regex.IsMatch(prop.Path, WildCardToRegular(item), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)))
                list.Add(item);

            MaskFieldsFromJToken(item, blacklist, mask);
        }

        foreach (JProperty item2 in list)
            item2.Value = mask;
    }

    private static string WildCardToRegular(string value)
    {
        return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
    }
}