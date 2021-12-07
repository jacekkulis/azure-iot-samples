// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices;
using System.Threading.Tasks;

namespace Azure.IoT.Samples
{
    public class BackendApplicationSample
    {
        private readonly BackendApplicationSettings _backendApplicationSettings;
        private readonly CloudMessageSender _sender;
        private readonly FeedbackReceiveRunner _feedback;

        public BackendApplicationSample(BackendApplicationSettings backendApplicationSettings)
        {
            _backendApplicationSettings = backendApplicationSettings;
            var serviceClient = ServiceClient.CreateFromConnectionString(_backendApplicationSettings.ConnectionString);
            _feedback = new FeedbackReceiveRunner(serviceClient);
            _sender = new CloudMessageSender(backendApplicationSettings, serviceClient);
        }

        public async Task RunSampleAsync()
        {
            _sender.Send();
            await _feedback.Run();
        }
    }
}
