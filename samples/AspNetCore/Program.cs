var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWeatherForecastRepository, WeatherForecastRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add ICacheService
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddCacheService(op =>
{
    op.DefaultOptions.Memory.RefreshInterval = TimeSpan.FromMinutes(1);
    op.DefaultOptions.Distributed.RefreshInterval = TimeSpan.FromMinutes(1);
});
////////////////////

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
