using MagicLogger.Helpers;
using Newtonsoft.Json;

namespace MagicLogger.Data;

public class ScopedAdditionalInfoLogger
{
    private readonly Dictionary<string, object> _dataScoped = new Dictionary<string, object>();

    public void AddItem(string key, object value)
    {
        _dataScoped[key] = value;
    }

    public void AddJsonItem<T>(string key, T value)
    {
        var jsonValue = JsonConvert.SerializeObject(value);
        _dataScoped[key] = jsonValue.DeserializeAsObject();
    }

    internal IEnumerable<KeyValuePair<string, object>> GetData()
    {
        foreach (var itemScoped in _dataScoped)
            yield return itemScoped;
    }
}
