using SilvermineNordic.Repository;
using SilvermineNordic.Models;
using SilvermineNordic.Repository.Services;
using SilvermineNordic.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

string snowMakingSqlConnectionString = config.GetConnectionString("SnowMakingSqlConnectionString");
string openWeatherApiKey = config.GetConnectionString("OpenWeatherApiForecastApiKey");
string emailServiceConnectionString = config.GetConnectionString("EmailServiceConnectionString");

var protocol = "https";
string inMemoryDatabaseName = null;
if (builder.Environment.IsDevelopment())
{
    inMemoryDatabaseName = "DevelopmentInMemoryDatabase";
    protocol = "http";
}

builder.Services.AddSingleton<ISilvermineNordicConfiguration>(_ =>
                new SilvermineNordicConfigurationService()
                {
                    SqlConnectionString = snowMakingSqlConnectionString,
                    OpenWeatherApiKey = openWeatherApiKey,
                    EmailServiceConnectionString = emailServiceConnectionString,
                    InMemoryDatabaseName = inMemoryDatabaseName,
                    Protocol = protocol,
                });

builder.Services.AddDbContext<SilvermineNordicDbContext>();

builder.Services.AddScoped<IRepositorySensorReading, EntityFrameworkSensorReadingService>();
builder.Services.AddScoped<IRepositoryThreshold, EntityFrameworkThresholdService>();
builder.Services.AddScoped<IWeatherForecast, OpenWeatherApiForecastService>();
builder.Services.AddScoped<IRepositoryUser, EntityFrameworkUserService>();
builder.Services.AddScoped<IRepositoryUserOtp, EntityFrameworkUserOtpService>();
builder.Services.AddScoped<IEmailService, AzureEmailService>();
builder.Services.AddScoped<IRepositoryUserOtp, EntityFrameworkUserOtpService>();
builder.Services.AddScoped<IRepositoryWeatherSensorThresholdCombined, WeatherSensorThresholdCombinedService>();

var serviceProvider = builder.Services.BuildServiceProvider();
var sensorReadingService = serviceProvider.GetService<IRepositorySensorReading>();
var sensorThresholdService = serviceProvider.GetService<IRepositoryThreshold>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("https://snowmaking.silverminenordic.com",
                                "https://snowmakingdev.silverminenordic.com",
                                "http://localhost:5290");
        });
});

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    sensorReadingService.SeedData();
    sensorThresholdService.SeedData();
}

//app.UseHttpsRedirection();

app.MapGet("/sensorreading/", async ([FromServices] IRepositorySensorReading sensorReadingService, int count) =>
{
    count = count > 50 ? 50 : count;
    count = count < 1 ? 1 : count;
    return await sensorReadingService.GetLastNReadingAsync(SensorReadingTypeEnum.Sensor, count);
}).WithName("GetLastSensorReading");

app.MapGet("/weatherreading/", async ([FromServices] IRepositorySensorReading sensorReadingService, int count) =>
{
    count = count > 50 ? 50 : count;
    count = count < 1 ? 1 : count;
    return await sensorReadingService.GetLastNReadingAsync(SensorReadingTypeEnum.Weather, count);
}).WithName("GetLastWeatherReading");

app.MapGet("/weatherforecast", async ([FromServices] IMemoryCache memoryCacheService, [FromServices] IWeatherForecast weatherForecastService) =>
{
    var weatherForecast = memoryCacheService.Get<IEnumerable<WeatherModel>>("WeatherForecast");
    if (weatherForecast == null)
    {
        weatherForecast = await weatherForecastService.GetWeatherForecast();
        memoryCacheService.Set("WeatherForecast", weatherForecast, DateTimeOffset.UtcNow.AddMinutes(14));
    }
    return weatherForecast;
}).WithName("GetWeatherForecast");

app.MapGet("thresholds", async ([FromServices] IRepositoryThreshold sensorThresholdService) =>
{
    var thresholds = await sensorThresholdService.GetThresholds();
    return thresholds;
});

app.MapGet("weathersensorthreshold", async ([FromServices] IRepositoryWeatherSensorThresholdCombined weatherSensorThresholdCombinedService, int count) =>
{
    count = count > 50 ? 50 : count;
    count = count < 1 ? 1 : count;
    var weatherSensorThresholdCombinedModel = await weatherSensorThresholdCombinedService.GetWeatherSensorThresholdCombined(count);
    return weatherSensorThresholdCombinedModel;
});

app.MapGet("/weatherforecast/nextzonechange", async (
    [FromServices] IMemoryCache memoryCacheService,
    [FromServices] IWeatherForecast weatherForecastService,
    [FromServices] IRepositoryWeatherSensorThresholdCombined weatherSensorThresholdCombinedService) =>
{
    var weatherForecast = memoryCacheService.Get<IEnumerable<WeatherModel>>("WeatherForecast");
    if (weatherForecast == null)
    {
        weatherForecast = await weatherForecastService.GetWeatherForecast();
        memoryCacheService.Set("WeatherForecast", weatherForecast, DateTimeOffset.UtcNow.AddMinutes(14));
    }
    var weatherSensorThresholdCombinedModel = await weatherSensorThresholdCombinedService.GetWeatherSensorThresholdCombined(1);
    var thresholdTask = weatherSensorThresholdCombinedModel.Thresholds;

    var lastSensorReading = weatherSensorThresholdCombinedModel.SensorReadings.SingleOrDefault();
    var lastWeatherReading = weatherSensorThresholdCombinedModel.WeatherReadings.SingleOrDefault();

    var inTheZone = false;
    if (lastSensorReading != null)
    {
        inTheZone = inTheZone || InTheZoneService.IsInZone(thresholdTask, lastSensorReading.TemperatureInCelcius, lastSensorReading.Humidity);
    }
    if (lastWeatherReading != null)
    {
        inTheZone = inTheZone || InTheZoneService.IsInZone(thresholdTask, lastWeatherReading.TemperatureInCelcius, lastWeatherReading.Humidity);
    }

    var nextZoneChangeDateTimeUtc = InTheZoneService.GetNextZoneChange(
        weatherForecast,
        thresholdTask,
        inTheZone);
    return nextZoneChangeDateTimeUtc;
}).WithName("GetNextZoneChange");

app.MapPost("loginattempt", async (
    [FromServices] IRepositoryUser userService,
    [FromServices] IRepositoryUserOtp userOtpService,
    [FromServices] IEmailService emailService,
    [FromBody] string login) =>
{
    try
    {
        var user = await userService.GetUserAsync(login);
        if (user != null)
        {
            var userOtpResult = await userOtpService.AddUserOtpAsync(user.Id);
            var emailResult = await emailService.SendEmailAsync(user.Email, "Snow Making Login Request", @$"Dear {(!string.IsNullOrWhiteSpace(user.Name) ? user.Name : user.Email)},{Environment.NewLine}<br/>https://snowmaking.silverminenordic.com/loginotp/{userOtpResult.Otp}");
            return true;
        }
        return true;
    }
    catch
    {
        return false;
    }
});

app.MapGet("loginotp", async ([FromServices] IRepositoryUserOtp userOtpService, string otp) =>
{
    if (Guid.TryParse(otp, out Guid otpGuid))
    {
        return await userOtpService.GetUserOtpAsync(otpGuid);
    }
    return (UserOtp?)null;
});

app.MapGet("loginauth", async ([FromServices] IRepositoryUserOtp userOtpService, string authKey) =>
{
    if (Guid.TryParse(authKey, out Guid authKeyGuid))
    {
        return await userOtpService.GetUserOtpByAuthKeyAsync(authKeyGuid);
    }
    return (User?)null;
});

app.Run();

