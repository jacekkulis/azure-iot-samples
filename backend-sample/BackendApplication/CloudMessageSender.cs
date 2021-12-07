using Microsoft.Azure.Devices;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Azure.IoT.Samples
{
    public class CloudMessageSender
    {
        private readonly ServiceClient _serviceClient;
        private BackendApplicationSettings _backendApplicationSettings;

        public CloudMessageSender(BackendApplicationSettings backendApplicationSettings, ServiceClient serviceClient)
        {
            _backendApplicationSettings = backendApplicationSettings;
            _serviceClient = serviceClient;
        }

        public async Task Send()
        {
            await this.SendCloudToDeviceMessageAsync();
        }

        private async Task SendCloudToDeviceMessageAsync()
        {
            Console.WriteLine("Send Cloud-to-Device message\n");
            var commandMessage = new Message(Encoding.ASCII.GetBytes(_backendApplicationSettings.Message));
            commandMessage.Ack = DeliveryAcknowledgement.PositiveOnly;
            commandMessage.ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(10);
            await _serviceClient.SendAsync(_backendApplicationSettings.DeviceId, commandMessage, TimeSpan.Zero);
        }
    }
}
