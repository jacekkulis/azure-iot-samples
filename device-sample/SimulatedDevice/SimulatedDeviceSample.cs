using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.IoT.Helpers;
using Azure.IoT.SharedModels;

namespace Azure.IoT.Samples
{
    public class SimulatedDeviceSample : IDisposable
    {
        #region Properties
        public DeviceTwinProperties DeviceTwinProperties { get; set; } = new DeviceTwinProperties();
        public ITelemetry TemperatureTelemetry { get; set; } = new TemperatureTelemetry();
        public ITelemetry HumidityTelemetry { get; set; } = new HumidityTelemetry();

        public Alert Alert { get; set; } = new Alert();

        #endregion

        #region Private fields

        private readonly object _syncObj = new object();
        private readonly DeviceClient _deviceClient;

        private CancellationTokenSource _tokenSource;
        private SimulationConfig _simConfig;

        private bool _simulationRunning, _readerRunning;

        #endregion Private fields

        public SimulatedDeviceSample(Settings settings)
        {
            IAuthenticationMethod auth;
            if (string.IsNullOrEmpty(settings.DeviceCertificateFilePath))
            {
                auth = new DeviceAuthenticationWithSharedAccessPolicyKey(settings.DeviceId, settings.AccessPolicyName, settings.AccessKey);
            }
            else
            {
                var certificate = CertificateLoader.LoadCertificateFromFile(settings.DeviceCertificateFilePath);
                auth = new DeviceAuthenticationWithX509Certificate(settings.DeviceId, certificate);
            }

            this._deviceClient = DeviceClient.Create(settings.HubName, auth, TransportType.Mqtt);

            if (_deviceClient == null)
            {
                throw new Exception("Failed to create DeviceClient!");
            }

            this._simConfig = settings.SimulationConfig;
        }

        #region Public methods

        public async Task ConnectAsync()
        {
            await _deviceClient.OpenAsync();

            await SetHandlers();
            await GetTwinDesired();
        }

        public void RunDevice()
        {
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();

            SimulateTelemetryData(_tokenSource.Token).ContinueWith((t) =>
            {
                _simulationRunning = false;
            });


            SimulateAlertData(_tokenSource.Token).ContinueWith((t) =>
            {
                _simulationRunning = false;
            });

            ReceiveUpdates(_tokenSource.Token).ContinueWith((t) =>
            {
                _readerRunning = false;
            });
        }


        #endregion

        #region Private methods

        private async Task SetHandlers()
        {
            await _deviceClient.SetMethodHandlerAsync("StartDevice", StartHandler, null);
            await _deviceClient.SetMethodHandlerAsync("StopDevice", StopHandler, null);
            await _deviceClient.SetMethodHandlerAsync("SetConfig", SetConfigHandler, null);
            await _deviceClient.SetMethodDefaultHandlerAsync(DefaultHandler, null);

            await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(TwinUpdateHandler, null);
        }

        private async Task GetTwinDesired()
        {
            var twin = await _deviceClient.GetTwinAsync();
            var properties = twin.Properties.Desired;
            Console.WriteLine($"Current twin (desired): {properties.ToJson()}\n");

            DeviceTwinProperties.Power = properties.Contains(nameof(DeviceTwinProperties.Power)) ? properties[nameof(DeviceTwinProperties.Power)] : 0;
            DeviceTwinProperties.Interval = properties.Contains(nameof(DeviceTwinProperties.Interval)) ? properties[nameof(DeviceTwinProperties.Interval)] : 5000;
        }

        private async Task UpdateTwinReported()
        {
            var twin = new
            {
                interval = DeviceTwinProperties.Interval,
                power = DeviceTwinProperties.Power
            };

            await _deviceClient.UpdateReportedPropertiesAsync(new TwinCollection(JsonConvert.SerializeObject(twin)));
        }

        private async Task SimulateTelemetryData(CancellationToken token)
        {
            Console.WriteLine("Device sending telemetry to IoTHub...\n");
            _simulationRunning = true;

            while (!token.IsCancellationRequested)
            {
                lock (_syncObj)
                {
                    TemperatureTelemetry = DataGenerator.NextTelemetry("Temperature", _simConfig.TemperatureMin, _simConfig.TemperatureMax, DeviceTwinProperties.Power);
                    HumidityTelemetry = DataGenerator.NextTelemetry("Humidity", _simConfig.HumidityMin, _simConfig.HumidityMin, DeviceTwinProperties.Power);

                }

                var data = new
                {
                    temperature = TemperatureTelemetry.Value,
                    humidity = HumidityTelemetry.Value
                };
                var dataBuffer = JsonConvert.SerializeObject(data);
                Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Sending telemetry: {dataBuffer}");

                var message = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                message.Properties.Add("messageType", MessageTypes.Telemetry);

                await _deviceClient.SendEventAsync(message, token);
                await Task.Delay(TimeSpan.FromMilliseconds(DeviceTwinProperties.Interval), token);
            }
        }

        private async Task SimulateAlertData(CancellationToken token)
        {
            Console.WriteLine("Device sending events to IoTHub...\n");
            _simulationRunning = true;

            while (!token.IsCancellationRequested)
            {
                lock (_syncObj)
                {
                    Alert = DataGenerator.NextAlert();
                }

                var data = new
                {
                    alarm = Alert
                };

                var dataBuffer = JsonConvert.SerializeObject(data);
                Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Sending event: {dataBuffer}");

                var message = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                message.Properties.Add("messageType", MessageTypes.Alert);

                await _deviceClient.SendEventAsync(message, token);
                await Task.Delay(TimeSpan.FromSeconds(_simConfig.EventInterval), token);
            }
        }

        private async Task ReceiveUpdates(CancellationToken token)
        {
            Console.WriteLine("\nDevice waiting for updates from IoTHub...\n");
            _readerRunning = true;

            while (!token.IsCancellationRequested)
            {
                using (var receivedMessage = await _deviceClient.ReceiveAsync(TimeSpan.FromSeconds(10)))
                {
                    if (receivedMessage != null)
                    {
                        var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                        Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Received message: {messageData}");

                        await _deviceClient.CompleteAsync(receivedMessage, token);
                    }
                }

                await Task.Delay(1000, token);
            }
        }

        #endregion

        #region Handlers

        private async Task<MethodResponse> StartHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"Service call {methodRequest.Name} - params: {methodRequest.DataAsJson}\n");

            RunDevice();

            return new MethodResponse(0);
        }

        private async Task<MethodResponse> StopHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"Service call {methodRequest.Name} - params: {methodRequest.DataAsJson}\n");

            _tokenSource?.Cancel();
            while (_simulationRunning || _readerRunning)
                ;

            return new MethodResponse(0);
        }

        private async Task<MethodResponse> SetConfigHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"Service call {methodRequest.Name} - params: {methodRequest.DataAsJson}\n");

            _simConfig = JsonConvert.DeserializeObject<SimulationConfig>(methodRequest.DataAsJson);

            return new MethodResponse(0);
        }

        private async Task<MethodResponse> DefaultHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"Service call {methodRequest.Name} - params: {methodRequest.DataAsJson}\n");
            return new MethodResponse(0);
        }

        private async Task TwinUpdateHandler(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Twin desired properties update: {desiredProperties.ToJson()}");
            if (desiredProperties.Contains(nameof(DeviceTwinProperties.Interval)))
            {
                DeviceTwinProperties.Interval = desiredProperties[nameof(DeviceTwinProperties.Interval)];
            }
            if (desiredProperties.Contains(nameof(DeviceTwinProperties.Power)))
            {
                lock (_syncObj)
                {
                    DeviceTwinProperties.Power = desiredProperties[nameof(DeviceTwinProperties.Power)];
                }
            }

            await UpdateTwinReported();
        }

        #endregion Handlers

        public void Dispose()
        {
            _tokenSource?.Cancel();
            while (_simulationRunning || _readerRunning)
                ;
            _tokenSource?.Dispose();
        }
    }
}