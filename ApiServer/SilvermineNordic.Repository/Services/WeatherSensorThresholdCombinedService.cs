using SilvermineNordic.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SilvermineNordic.Repository.Services
{
    public class WeatherSensorThresholdCombinedService : IRepositoryWeatherSensorThresholdCombined
    {
        private readonly ISilvermineNordicConfiguration _configurationService;
        private readonly SilvermineNordicDbContext _dbContext;
        public WeatherSensorThresholdCombinedService(
            ISilvermineNordicConfiguration configurationService,
            SilvermineNordicDbContext dbContext)
        {
            _configurationService = configurationService;
            _dbContext = dbContext;
        }

        //         public async Task<WeatherSensorThresholdCombinedModel> GetWeatherSensorThresholdCombined(int count)
        //         {
        //             var sql = $@"
        // SELECT TOP {count} *
        // FROM SensorReading
        // WHERE Type = '{SensorReadingTypeEnum.Sensor}'
        // ORDER BY Id DESC;
        // SELECT TOP {count} *
        // FROM SensorReading
        // WHERE Type = '{SensorReadingTypeEnum.Weather}'
        // ORDER BY Id DESC;
        // SELECT *
        // FROM SensorThreshold;
        //             ";
        //             using (var db = new SqlConnection(_configurationService.GetSqlConnectionString()))
        //             {
        //                 var model = new WeatherSensorThresholdCombinedModel();
        //                 var query = await SqlMapper.QueryMultipleAsync(db, sql);
        //                 model.SensorReadings = query.Read<SensorReading>().ToList();
        //                 model.WeatherReadings = query.Read<SensorReading>().ToList();
        //                 model.Thresholds = query.Read<Threshold>().ToList();
        //                 return model;
        //             }
        //         }

        public async Task<WeatherSensorThresholdCombinedModel> GetWeatherSensorThresholdCombined(int count)
        {
            var sensorReadingsTask = _dbContext.SensorReadings
                .Where(_ => _.Type == SensorReadingTypeEnum.Sensor.ToString())
                .OrderByDescending(_ => _.ReadingDateTimestampUtc)
                .Take(count)
                .ToListAsync();
            var weatherReadingsTask = _dbContext.SensorReadings
                .Where(_ => _.Type == SensorReadingTypeEnum.Weather.ToString())
                .OrderByDescending(_ => _.ReadingDateTimestampUtc)
                .Take(count)
                .ToListAsync();
            var thresholdsTask = _dbContext.Thresholds
                .OrderBy(_ => _.TemperatureInCelciusLowThreshold)
                .ToListAsync();
            return new WeatherSensorThresholdCombinedModel()
            {
                SensorReadings = await sensorReadingsTask,
                WeatherReadings = await weatherReadingsTask,
                Thresholds = await thresholdsTask,
            };
        }
    }
}