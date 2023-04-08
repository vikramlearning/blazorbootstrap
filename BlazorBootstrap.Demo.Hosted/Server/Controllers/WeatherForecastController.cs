using Microsoft.AspNetCore.Mvc;

namespace BlazorBootstrap.Demo.Hosted.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello!";
}