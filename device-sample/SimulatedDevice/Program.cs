using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Azure.IoT.SharedModels;

namespace Azure.IoT.Samples
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();


            var settings = config.Get<Settings>();

            Console.WriteLine("Starting simulator.");

            using (var device = new SimulatedDeviceSample(settings))
            {
                await device.ConnectAsync();
                device.RunDevice();

                Console.WriteLine("Started - press enter to stop\n");
                Console.ReadLine();
            }
            Console.WriteLine("Stopped - press enter to exit\n");
            Console.ReadLine();
        }
    }
}