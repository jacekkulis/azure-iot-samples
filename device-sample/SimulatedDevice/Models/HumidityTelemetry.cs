using System;

namespace Azure.IoT.Samples
{
    public class HumidityTelemetry : ITelemetry
    {
        public string Name { get; set; } = "Humidity";
        public object Value { get; set; }
        public string DataSource { get; set; } = "Sensor2";
        public string DataType { get; set; } = "int";
        public DateTimeOffset Timestamp { get; set; }
        public int MinRange { get; set; }
        public int MaxRange { get; set; }
        public string Unit { get; set; }
        public int Frequency { get; set; }
    }
}
