using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.IoT.Samples
{
    public class FeedbackReceiveRunner
    {
        private readonly ServiceClient _serviceClient;

        public FeedbackReceiveRunner(ServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public async Task Run()
        {
            Console.WriteLine("\nReceiving c2d feedback from service");
            this.ReceiveFeedback();
        }

        private async void ReceiveFeedback()
        {
            var feedbackReceiver = _serviceClient.GetFeedbackReceiver();

            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();

                if (feedbackBatch == null)
                    continue;

                var positiveFeedback = feedbackBatch.Records.Where(f => f.StatusCode.Equals(FeedbackStatusCode.Success));
                var negativeFeedback = feedbackBatch.Records.Where(f => f.StatusCode != FeedbackStatusCode.Success);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received positive feedback: {0}", string.Join(", ", JsonConvert.SerializeObject(positiveFeedback)));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Received negative feedback: {0}", string.Join(", ", JsonConvert.SerializeObject(negativeFeedback)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }
    }
}
