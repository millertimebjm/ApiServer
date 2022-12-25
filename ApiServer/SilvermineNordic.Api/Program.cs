using SilvermineNordic.Repository;
using SilvermineNordic.Models;
using SilvermineNordic.Repository.Services;
using SilvermineNordic.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

string snowMakingSqlConnectionString = config.GetConnectionString("SnowMakingSqlConnectionString");
string openWeatherApiKey = config.GetConnectionString("OpenWeatherApiForecastApiKey");
string emailServiceConnectionString = config.GetConnectionString("EmailServiceConnectionString");

builder.Services.AddSingleton<ISilvermineNordicConfiguration>(_ =>
                new SilvermineNordicConfigurationService()
                {
                    SqlConnectionString = snowMakingSqlConnectionString,
                    OpenWeatherApiKey = openWeatherApiKey,
                    EmailServiceConnectionString = emailServiceConnectionString,
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

var sensorReadingService = builder.Services.BuildServiceProvider().GetService<IRepositorySensorReading>();
var sensorThresholdService = builder.Services.BuildServiceProvider().GetService<IRepositoryThreshold>();
var weatherForecastService = builder.Services.BuildServiceProvider().GetService<IWeatherForecast>();
var userService = builder.Services.BuildServiceProvider().GetService<IRepositoryUser>();
var userOtpService = builder.Services.BuildServiceProvider().GetService<IRepositoryUserOtp>();
var emailService = builder.Services.BuildServiceProvider().GetService<IEmailService>();
var weatherSensorThresholdCombinedService = builder.Services.BuildServiceProvider().GetService<IRepositoryWeatherSensorThresholdCombined>();

builder.Services.AddCors(o => o.AddPolicy("NUXT", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

var app = builder.Build();

app.UseCors("NUXT");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapGet("/sensorreading/", async (int count) =>
{
    count = count > 100 ? 100 : count;
    count = count < 1 ? 1 : count;
    return await sensorReadingService.GetLastNReadingAsync(SensorReadingTypeEnum.Sensor, count);
}).WithName("GetLastSensorReading");

app.MapGet("/weatherreading/", async (int count) =>
{
    count = count > 100 ? 100 : count;
    count = count < 1 ? 1 : count;
    return await sensorReadingService.GetLastNReadingAsync(SensorReadingTypeEnum.Weather, count);
}).WithName("GetLastWeatherReading");

app.MapGet("/weatherforecast", async () =>
{
    return await weatherForecastService.GetWeatherForecast();
}).WithName("GetWeatherForecast");

app.MapGet("thresholds", async () =>
{
    var thresholds = await sensorThresholdService.GetThresholds();
    return thresholds;
});

app.MapGet("weathersensorthreshold", async (int count) =>
{
    var weatherSensorThresholdCombinedModel = await weatherSensorThresholdCombinedService.GetWeatherSensorThresholdCombined(count);
    return weatherSensorThresholdCombinedModel;
});

app.MapGet("/weatherforecast/nextzonechange", async () =>
{
    var weatherForecastTask = await weatherForecastService.GetWeatherForecast();
    var thresholdTask = await sensorThresholdService.GetThresholds();
    var lastSensorReadingTask = await sensorReadingService.GetLastNReadingAsync(SensorReadingTypeEnum.Sensor, 1);
    var lastWeatherReadingTask = await sensorReadingService.GetLastNReadingAsync(SensorReadingTypeEnum.Weather, 1);
    //await Task.WhenAll(weatherForecastTask, thresholdTask, lastSensorReadingTask, lastWeatherReadingTask);

    var lastSensorReading = lastSensorReadingTask.Single();
    var lastWeatherReading = lastWeatherReadingTask.Single();
    var nextZoneChangeDateTimeUtc = InTheZoneService.GetNextZoneChange(weatherForecastTask, thresholdTask, InTheZoneService.IsInZone(thresholdTask, lastSensorReading.TemperatureInCelcius, lastSensorReading.Humidity) || InTheZoneService.IsInZone(thresholdTask, lastWeatherReading.TemperatureInCelcius, lastWeatherReading.Humidity));
    return nextZoneChangeDateTimeUtc;
}).WithName("GetNextZoneChange");

app.MapPost("loginattempt", async ([Microsoft.AspNetCore.Mvc.FromBody] string login) =>
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
    catch (Exception ex)
    {
        return false;
    }
});

app.MapGet("loginotp", async (string otp) =>
{
    if (Guid.TryParse(otp, out Guid otpGuid))
    {
        return await userOtpService.GetUserOtpAsync(otpGuid);
    }
    return (UserOtp)null;
});

app.MapGet("loginauth", async (string authKey) =>
{
    if (Guid.TryParse(authKey, out Guid authKeyGuid))
    {
        return await userOtpService.GetUserOtpByAuthKeyAsync(authKeyGuid);
    }
    return (User)null;
});

app.Run();

