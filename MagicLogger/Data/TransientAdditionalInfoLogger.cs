using MagicLogger.Helpers;
using Newtonsoft.Json;

namespace MagicLogger.Data;

public class TransientAdditionalInfoLogger
{
    private readonly Dictionary<string, object> _dataTransient = new Dictionary<string, object>();

    public void AddItem(string key, object value)
    {
        _dataTransient[key] = value;
    }

    public void AddJsonItem<T>(string key, T value)
    {
        var jsonValue = JsonConvert.SerializeObject(value);
        _dataTransient[key] = jsonValue.DeserializeAsObject();
    }

    internal IEnumerable<KeyValuePair<string, object>> GetData()
    {
        foreach (var itemTransient in _dataTransient)
            yield return itemTransient;
    }

    internal void ClearData()
    {
        _dataTransient.Clear();
    }
}

public class Temp<T>
{
    private readonly ILogger<T> _logger;
    private readonly TransientAdditionalInfoLogger _additionalInfo;

    public Temp(ILogger<T> logger, TransientAdditionalInfoLogger additionalInfo)
    {
        _logger = logger;
        _additionalInfo = additionalInfo;
    }

    public void LogInformation(Dictionary<string, object> additionalInfo, string? message, params object[] args)
    {
        foreach (var item in additionalInfo)
            _additionalInfo.AddItem(item.Key, item.Value);

        _logger.LogInformation(message, args);
    }
}