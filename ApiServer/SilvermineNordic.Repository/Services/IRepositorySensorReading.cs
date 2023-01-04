using SilvermineNordic.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SilvermineNordic.Repository.Services
{
    public interface IRepositorySensorReading
    {
        public Task<SensorReading> AddSensorReadingAsync(SensorReading sensorReading);
        public Task<IEnumerable<SensorReading>> GetLastNReadingAsync(SensorReadingTypeEnum type, int count);
        public Task SeedData();
    }
}

