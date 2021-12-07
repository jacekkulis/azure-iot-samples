// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Azure.IoT.Helpers;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System;
using Azure.IoT.SharedModels;

namespace Azure.IoT.Samples
{
    public static class Program
    {
        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";

        public static int Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var settings = config.Get<Settings>();

            if (string.IsNullOrWhiteSpace(settings.DpsIdScope) && (args.Length > 0))
            {
                settings.DpsIdScope = args[0];
            }

            if (string.IsNullOrWhiteSpace(settings.DpsIdScope))
            {
                Console.WriteLine("ProvisioningDeviceClientX509 <IDScope>");
                return 1;
            }

            var certificate = CertificateLoader.LoadCertificateFromFile(settings.DeviceCertificateFilePath);

            using var security = new SecurityProviderX509Certificate(certificate);
            using (var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.TcpOnly))
            {
                var provClient = ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, settings.DpsIdScope, security, transport);
                var sample = new ProvisioningDeviceSample(provClient, security);
                sample.RunSampleAsync().GetAwaiter().GetResult();
            }

            return 0;
        }
    }
}
