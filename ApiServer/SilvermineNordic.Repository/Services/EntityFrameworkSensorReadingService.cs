using Microsoft.EntityFrameworkCore;
using SilvermineNordic.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SilvermineNordic.Repository.Services
{
    public class EntityFrameworkSensorReadingService : IRepositorySensorReading
    {
        private readonly SilvermineNordicDbContext _dbContext;
        public EntityFrameworkSensorReadingService(SilvermineNordicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SensorReading> AddSensorReadingAsync(SensorReading sensorReading)
        {
            await _dbContext.SensorReadings.AddAsync(sensorReading);
            await _dbContext.SaveChangesAsync();
            return sensorReading;
        }

        public async Task<IEnumerable<SensorReading>> GetLastNReadingAsync(SensorReadingTypeEnum type, int count)
        {
            return await _dbContext.SensorReadings
                .Where(_ => _.Type == type.ToString())
                .OrderByDescending(_ => _.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task SeedData()
        {
            await _dbContext.SensorReadings.AddAsync(new SensorReading()
            {
                Id = 1,
                DateTimestampUtc = DateTime.UtcNow,
                Humidity = 88.8m,
                InsertedDateTimestampUtc = DateTime.UtcNow,
                ReadingDateTimestampUtc = DateTime.UtcNow,
                TemperatureInCelcius = 10m,
                Type = SensorReadingTypeEnum.Sensor.ToString(),
            });
            await _dbContext.SensorReadings.AddAsync(new SensorReading()
            {
                Id = 2,
                DateTimestampUtc = DateTime.UtcNow,
                Humidity = 77.7m,
                InsertedDateTimestampUtc = DateTime.UtcNow,
                ReadingDateTimestampUtc = DateTime.UtcNow,
                TemperatureInCelcius = 15m,
                Type = SensorReadingTypeEnum.Weather.ToString(),
            });
            await _dbContext.SaveChangesAsync();
        }
    }
}
