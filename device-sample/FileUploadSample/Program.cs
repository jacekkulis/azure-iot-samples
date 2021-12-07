// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Azure.IoT.Helpers;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Samples;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Azure.IoT.SharedModels;

namespace Azure.IoT.Samples
{
    /// <summary>
    /// This sample requires an IoT Hub linked to a storage account container.
    /// Find instructions to configure a hub at <see href="https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-configure-file-upload"/>.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// A sample to illustrate how to upload files from a device.
        /// </summary>
        /// <param name="args">
        /// Run with `--help` to see a list of required and optional parameters.
        /// </param>
        /// <returns></returns>
        public static async Task<int> Main(string[] args)
        {
            // Parse application parameters
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var settings = config.Get<Settings>();

            var certificate = CertificateLoader.LoadCertificateFromFile(settings.DeviceCertificateFilePath);
            var auth = new DeviceAuthenticationWithX509Certificate(settings.DeviceId, certificate);

            using var deviceClient = DeviceClient.Create(settings.HubName, auth, TransportType.Mqtt);

            var sample = new FileUploadSample(deviceClient);
            await sample.RunSampleAsync();

            Console.WriteLine("Done.");
            return 0;
        }
    }
}
