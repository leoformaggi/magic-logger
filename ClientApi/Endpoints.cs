using MagicLogger.Data;

internal static class Endpoints
{
    private static string[] _summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public static WeatherForecast[] GetWeather(ILogger<WeatherForecast> logger, TransientAdditionalInfoLogger addInfo)
    {
        addInfo.AddItem("Metodo", "MapGet /weatherforecast");
        logger.LogInformation("Testando log injetado no {Metodo}");

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateTime.Now.AddDays(index),
                Random.Shared.Next(-20, 55),
                _summaries[Random.Shared.Next(_summaries.Length)]
            ))
            .ToArray();

        //throw new Exception();

        return forecast;
    }
}