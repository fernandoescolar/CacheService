namespace CacheService.Samples.AspNetCore.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastRepository _repository;
    private readonly ICacheService _cache;

    public WeatherForecastController(IWeatherForecastRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IActionResult> GetAsync()
    {
        var model = await _cache.GetOrSetAsync("forecast", ct => _repository.GetAsync(), HttpContext.RequestAborted);
        return Ok(model);
    }
}
