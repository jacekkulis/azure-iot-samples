// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub service SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/service
using Microsoft.Extensions.Configuration;
using System;

namespace Azure.IoT.Samples
{
    class Program
    {
        private static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var settings = config.Get<BackendApplicationSettings>();

            var sample = new BackendApplicationSample(settings);

            sample.RunSampleAsync().GetAwaiter().GetResult();
            Console.ReadLine();
        }
    }
}
