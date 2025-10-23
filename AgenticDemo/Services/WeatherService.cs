using System.Text.Json.Serialization;

namespace AgenticDemo.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://api.open-meteo.com/v1/forecast";
    private const double Latitude = 33.4483; // Phoenix, AZ area
    private const double Longitude = -112.0725;

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherForecast[]> GetWeatherForecastAsync()
    {
        try
        {
            var url = $"{ApiBaseUrl}?latitude={Latitude}&longitude={Longitude}&daily=temperature_2m_max,temperature_2m_min&timezone=America/Phoenix&forecast_days=5";

            var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);

            if (response?.Daily?.Time == null || response.Daily.Temperature2mMax == null || response.Daily.Temperature2mMin == null)
            {
                return GetFallbackData();
            }

            var forecasts = new List<WeatherForecast>();
            for (int i = 0; i < response.Daily.Time.Length && i < 5; i++)
            {
                if (DateOnly.TryParse(response.Daily.Time[i], out var date))
                {
                    var maxTemp = response.Daily.Temperature2mMax[i];
                    var minTemp = response.Daily.Temperature2mMin[i];
                    var avgTemp = (maxTemp + minTemp) / 2.0;

                    forecasts.Add(new WeatherForecast
                    {
                        Date = date,
                        TemperatureC = (int)Math.Round(avgTemp)
                    });
                }
            }

            return forecasts.ToArray();
        }
        catch (Exception)
        {
            // Return fallback data if API call fails
            return GetFallbackData();
        }
    }

    private WeatherForecast[] GetFallbackData()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
        }).ToArray();
    }
}

public class OpenMeteoResponse
{
    [JsonPropertyName("daily")]
    public Daily? Daily { get; set; }
}

public class Daily
{
    [JsonPropertyName("time")]
    public string[]? Time { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public double[]? Temperature2mMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public double[]? Temperature2mMin { get; set; }
}

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}