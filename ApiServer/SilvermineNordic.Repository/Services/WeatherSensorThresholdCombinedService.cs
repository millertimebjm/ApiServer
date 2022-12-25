using SilvermineNordic.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace SilvermineNordic.Repository.Services
{
    public class WeatherSensorThresholdCombinedService : IRepositoryWeatherSensorThresholdCombined
    {
        private readonly SilvermineNordicConfigurationService _configurationService;
        public WeatherSensorThresholdCombinedService(SilvermineNordicConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public async Task<WeatherSensorThresholdCombinedModel> GetWeatherSensorThresholdCombined(int count)
        {
            var sql = $@"
SELECT TOP 10 *
FROM SensorReading
WHERE Type = {SensorReadingTypeEnum.Sensor.ToString()}
ORDER BY Id DESC
UNION
SELECT TOP 10 *
FROM SensorReading
WHERE Type = {SensorReadingTypeEnum.Weather.ToString()}
ORDER BY Id DESC;
SELECT *
FROM SensorThreshold;
            ";
            using (var db = new SqlConnection(_connectionString))
            {
                var model = new WeatherSensorThresholdCombinedModel();
                using (var multi = db.QueryMultiple<WeatherSensorThresholdCombinedModel>(sql))
                {
                    model.SensorReadings = multi.Read<SensorReading>()
                        .Where(_ => _.Type == SensorReadingTypeEnum.Sensor.ToString())
                        .ToList();
                    model.WeatherReadings = multi.Read<SensorReading>()
                        .Where(_ => _.Type == SensorReadingTypeEnum.Weather.ToString())
                        .ToList();
                    model.Thresholds = multi.Read<Threshold>().ToList();
                }
                return model;
            }
        }
    }
}