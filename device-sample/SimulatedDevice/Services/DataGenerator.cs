
using System;

namespace Azure.IoT.Samples
{
    public static class DataGenerator
    {
        private static readonly Random Random = new Random();

        public static double NextDouble(int min, int max, double power)
        {
            return Random.Next(min, max) * (power / 100);
        }

        public static ITelemetry NextTelemetry(string type, int min, int max, double power)
        {
            return type switch
            {
                MessageTypes.Telemetry => new TemperatureTelemetry() { Value = NextDouble(min, max, power) },
                MessageTypes.Alert => new HumidityTelemetry() { Value = NextDouble(min, max, power) },
                _ => throw new ArgumentException()
            };
        }

        public static Alert NextAlert()
        {
            var alert = new Alert
            {
                AlertMessage = "Sample alert message",
                Timestamp = DateTimeOffset.UtcNow
            };

            return alert;
        }
    }
}
