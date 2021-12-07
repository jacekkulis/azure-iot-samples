// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Azure.IoT.Helpers;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Azure.IoT.Samples
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var settings = config.Get<DeviceClientStreamingSettings>();


            var certificate = CertificateLoader.LoadCertificateFromFile(settings.DeviceCertificateFilePath);
            var auth = new DeviceAuthenticationWithX509Certificate(settings.DeviceId, certificate);

            using var deviceClient = DeviceClient.Create(settings.HubName, auth, TransportType.Mqtt);

            var sample = new DeviceStreamSample(deviceClient);
            await sample.RunSampleAsync();

            Console.WriteLine("Done.");
            return 0;
        }
    }
}
