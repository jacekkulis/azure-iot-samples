// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Azure.IoT.Samples
{
    public class ProvisioningDeviceSample
    {
        readonly ProvisioningDeviceClient _provClient;
        readonly SecurityProvider _security;

        public ProvisioningDeviceSample(ProvisioningDeviceClient provisioningDeviceClient, SecurityProvider security)
        {
            _provClient = provisioningDeviceClient;
            _security = security;
        }

        public async Task RunSampleAsync()
        {
            Console.WriteLine($"RegistrationID = {_security.GetRegistrationID()}");
            VerifyRegistrationIdFormat(_security.GetRegistrationID());

            Console.Write("ProvisioningClient RegisterAsync . . . ");

            var result = await _provClient.RegisterAsync().ConfigureAwait(false);

            Console.WriteLine($"{result.Status}");
            Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");

            if (result.Status != ProvisioningRegistrationStatusType.Assigned) return;

            IAuthenticationMethod auth;

            if (_security is SecurityProviderX509)
            {
                Console.WriteLine("Creating X509 DeviceClient authentication.");
                auth = new DeviceAuthenticationWithX509Certificate(result.DeviceId, (_security as SecurityProviderX509).GetAuthenticationCertificate());
            }
            else
            {
                throw new NotSupportedException("Unknown authentication type.");
            }

            using var iotClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt_Tcp_Only);

            Console.WriteLine("DeviceClient OpenAsync.");
            await iotClient.OpenAsync().ConfigureAwait(false);

            Console.WriteLine("DeviceClient SendEventAsync.");
            await iotClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes("TestMessage"))).ConfigureAwait(false);

            Console.WriteLine("DeviceClient CloseAsync.");
            await iotClient.CloseAsync().ConfigureAwait(false);
        }

        private void VerifyRegistrationIdFormat(string v)
        {
            var r = new Regex("^[a-z0-9-]*$");
            if (!r.IsMatch(v))
            {
                throw new FormatException("Invalid registrationId: The registration ID is alphanumeric, lowercase, and may contain hyphens");
            }
        }
    }
}
