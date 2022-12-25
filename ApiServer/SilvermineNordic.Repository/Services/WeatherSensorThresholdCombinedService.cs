using SilvermineNordic.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Linq;

namespace SilvermineNordic.Repository.Services
{
    public class WeatherSensorThresholdCombinedService : IRepositoryWeatherSensorThresholdCombined
    {
        private readonly ISilvermineNordicConfiguration _configurationService;
        public WeatherSensorThresholdCombinedService(ISilvermineNordicConfiguration configurationService)
        {
            _configurationService = configurationService;
        }

        public async Task<WeatherSensorThresholdCombinedModel> GetWeatherSensorThresholdCombined(int count)
        {
            var sql = $@"
SELECT TOP {count} *
FROM SensorReading
WHERE Type = '{SensorReadingTypeEnum.Sensor}'
ORDER BY Id DESC;
SELECT TOP {count} *
FROM SensorReading
WHERE Type = '{SensorReadingTypeEnum.Weather}'
ORDER BY Id DESC;
SELECT *
FROM SensorThreshold;
            ";
            using (var db = new SqlConnection(_configurationService.GetSqlConnectionString()))
            {
                var model = new WeatherSensorThresholdCombinedModel();
                var query = await SqlMapper.QueryMultipleAsync(db, sql);
                model.SensorReadings = query.Read<SensorReading>().ToList();
                model.WeatherReadings = query.Read<SensorReading>().ToList();
                model.Thresholds = query.Read<Threshold>().ToList();
                return model;
            }
        }
    }
}