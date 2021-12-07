namespace Azure.IoT.SharedModels
{
    public class SimulationConfig
    {
        public int HumidityMin { get; set; }
        public int HumidityMax { get; set; }
        public int TemperatureMin { get; set; }
        public int TemperatureMax { get; set; }
        public int EventInterval { get; set; } = 30;
    }
}