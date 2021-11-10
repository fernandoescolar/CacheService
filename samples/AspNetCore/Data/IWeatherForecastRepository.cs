namespace CacheService.Samples.AspNetCore.Data;

public interface IWeatherForecastRepository
{
    ValueTask<WeatherForecast[]?> GetAsync();
}
