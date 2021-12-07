using System;

namespace Azure.IoT.Samples
{
    public interface ITelemetry
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string DataSource { get; set; }
        public string DataType { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int MinRange { get; set; }
        public int MaxRange { get; set; }
        public string Unit { get; set; }
        public int Frequency { get; set; }
    }
}
