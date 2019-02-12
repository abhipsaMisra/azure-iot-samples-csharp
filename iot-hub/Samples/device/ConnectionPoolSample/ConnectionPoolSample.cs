using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class ConnectionPoolSample
    {
        private static string _iothubConnectionString = Environment.GetEnvironmentVariable("IOTHUB_CONN_STRING_CSHARP");
        private static string _iothubDeviceConnectionString = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_LEVEL_CONN_STRING");
        
        private const int maxPoolSize = 1;
        private const bool pooling = true;

        private const int MessageCount = 1;

        private string _deviceId;
        private DeviceClient _deviceClient;

        public ConnectionPoolSample(string deviceId)
        {
            _deviceId = deviceId + Guid.NewGuid().ToString();
        }

        public async Task RunSampleAsync()
        {
            await GetDeviceAsync().ConfigureAwait(false);
            var transportSettings = GetTransportSettings();
            _deviceClient = DeviceClient.CreateFromConnectionString(_iothubDeviceConnectionString, _deviceId, transportSettings);

            await SendEventAsync().ConfigureAwait(false);

            await _deviceClient.SetMethodHandlerAsync(nameof(WriteToConsole), WriteToConsole, null).ConfigureAwait(false);
            Console.WriteLine("Waiting 60 seconds for IoT Hub method calls ...");
            Console.WriteLine($"Use the IoT Hub Azure Portal to call method {nameof(WriteToConsole)} for device {_deviceId} within this time.");
            await Task.Delay(60 * 1000).ConfigureAwait(false);

            await _deviceClient.CloseAsync().ConfigureAwait(false);
        }

        private async Task GetDeviceAsync()
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(_iothubConnectionString);
            Device device = await registryManager.GetDeviceAsync(_deviceId);

            if (device == null)
            {
                device = await registryManager.AddDeviceAsync(new Device(_deviceId));
            }
        }

        private async Task SendEventAsync()
        {
            EventHubTestListener testListener = await EventHubTestListener.CreateListener(_deviceId).ConfigureAwait(false);

            Console.WriteLine("Device {0} sending {1} messages to IoTHub...\n", _deviceId, MessageCount);

            try
            {
                for (int count = 0; count < MessageCount; count++)
                {
                    string payload = Guid.NewGuid().ToString();
                    string p1Value = Guid.NewGuid().ToString();

                    Console.WriteLine($"Message for device {_deviceId}: payload='{payload}' p1Value='{p1Value}'");

                    var message = new Message(Encoding.UTF8.GetBytes(payload))
                    {
                        Properties = { ["property1"] = p1Value }
                    };

                    await _deviceClient.SendEventAsync(message).ConfigureAwait(false);

                    bool isReceived = await testListener.WaitForMessage(_deviceId, payload, p1Value).ConfigureAwait(false);
                    if (isReceived)
                    {
                        Console.WriteLine($"Device {_deviceId}: Message {count} received.");
                    }
                    else
                    {
                        throw new Exception($"Device {_deviceId}: Message {count} not received.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Device {_deviceId} exception thrown: {ex.Message}");
            }
            finally
            {
                await testListener.CloseAsync().ConfigureAwait(false);
            }            
        }

        private Task<MethodResponse> WriteToConsole(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t *** {nameof(WriteToConsole)} was called.");

            Console.WriteLine();
            Console.WriteLine("\t{0}", methodRequest.DataAsJson);
            Console.WriteLine();

            return Task.FromResult(new MethodResponse(new byte[0], 200));
        }

        private ITransportSettings[] GetTransportSettings()
        {
            var tcpSettings = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            var webSocketSettings = new AmqpTransportSettings(TransportType.Amqp_WebSocket_Only);

            var amqpConnectionPoolSettings = new AmqpConnectionPoolSettings
            {
                MaxPoolSize = maxPoolSize,
                Pooling = pooling
            };

            tcpSettings.AmqpConnectionPoolSettings = amqpConnectionPoolSettings;
            webSocketSettings.AmqpConnectionPoolSettings = amqpConnectionPoolSettings;

            return new ITransportSettings[]
                {
                    tcpSettings,
                    webSocketSettings
                };
        }
    }
}
