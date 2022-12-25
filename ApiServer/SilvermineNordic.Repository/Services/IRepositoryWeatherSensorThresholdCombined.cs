using SilvermineNordic.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SilvermineNordic.Repository.Services
{
    public interface IRepositoryWeatherSensorThresholdCombined
    {
        public Task<WeatherSensorThresholdCombinedModel> GetWeatherSensorThresholdCombined(int count);
    }
}

