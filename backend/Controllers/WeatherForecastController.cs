using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<string> Get([FromServices] IDistributedCache cache)
    {
        _logger.LogInformation("hello from backend.");
        try
        {
            //var client = new BlobContainerClient(new Uri(https://127.0.0.1:10000/devstoreaccount1/container-name));

            var client = new BlobContainerClient(
                new Uri("http://127.0.0.1:10000/devstoreaccount1/container-name"),
                new StorageSharedKeyCredential("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));

            var resposne = await client.ExistsAsync();

            _logger.LogInformation($"blob container exists: {Response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error with blob container client");
        }

        var weather = await cache.GetStringAsync("weather");

        if (weather == null)
        {
            var rng = new Random();
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            weather = JsonSerializer.Serialize(forecasts);

            await cache.SetStringAsync("weather", weather, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            });
        }
        return weather;
    }
}
