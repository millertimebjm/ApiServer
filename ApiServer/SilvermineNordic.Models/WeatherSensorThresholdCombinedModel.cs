using System.Collections.Generic;

namespace SilvermineNordic.Models
{
    public class WeatherSensorThresholdCombinedModel
    {
        public IEnumerable<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
        public IEnumerable<SensorReading> WeatherReadings { get; set; } = new List<SensorReading>();
        public IEnumerable<Threshold> Thresholds { get; set; } = new List<Threshold>();
    }
}