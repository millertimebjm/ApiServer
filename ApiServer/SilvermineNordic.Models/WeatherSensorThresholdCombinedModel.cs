using System.Collections.Generic;

namespace SilvermineNordic.Models
{
    public class WeatherSensorThresholdCombinedModel
    {
        public List<SensorReading> SensorReadings { get; set; } = new();
        public List<SensorReading> WeatherReadings { get; set; } = new();
        public List<Threshold> Thresholds { get; set; } = new();
    }
}