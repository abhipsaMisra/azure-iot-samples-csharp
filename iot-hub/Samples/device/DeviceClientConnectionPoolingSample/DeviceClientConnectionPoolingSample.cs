// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class MessageSample
    {
        private const int MessageCount = 5;
        private const int TemperatureThreshold = 30;
        private static Random s_randomGenerator = new Random();
        private float _temperature;
        private float _humidity;
        private DeviceClient[] _deviceClientPool;
        private static TransportType s_transportType;
        private static string s_connectionString;
        private static int s_deviceCount;
        private static string s_prefix;

        public MessageSample(TransportType transportType, string connectionString, int deviceCount, string prefix)
        {
            s_transportType = transportType;
            s_connectionString = connectionString;
            s_deviceCount = deviceCount;
            s_prefix = prefix;
        }

        public async Task RunSampleAsync()
        {
            _deviceClientPool = await CreateDeviceClientOverMultiplex(s_transportType, s_connectionString, s_deviceCount, s_prefix).ConfigureAwait(false);
            foreach (DeviceClient deviceClient in _deviceClientPool)
            {
                await SendEvent(deviceClient).ConfigureAwait(false);
            }
            //await ReceiveCommands().ConfigureAwait(false);
        }

        private async Task<DeviceClient[]> CreateDeviceClientOverMultiplex(TransportType transportType, string connectionString, int deviceCount, string prefix)
        {
            DeviceClient[] deviceClientPool = new DeviceClient[deviceCount];
            Device[] devicePool = new Device[deviceCount];

            for (int index = 0; index < deviceCount; index++)
            {
                devicePool[index] = await CreateDeviceAsync(prefix, index, connectionString).ConfigureAwait(false);

                var auth = new DeviceAuthenticationWithRegistrySymmetricKey(devicePool[index].Id, devicePool[index].Authentication.SymmetricKey.PrimaryKey);
                deviceClientPool[index] = DeviceClient.Create(
                    GetHostName(connectionString),
                    auth,
                    new ITransportSettings[]
                    {
                        new AmqpTransportSettings(transportType)
                        {
                            AmqpConnectionPoolSettings = new AmqpConnectionPoolSettings()
                            {
                                Pooling = true,
                                MaxPoolSize = 10
                            }
                        }
                    });
                await deviceClientPool[index].OpenAsync().ConfigureAwait(true);
            }

            return deviceClientPool;
        }

        private async Task SendEvent(DeviceClient deviceClient)
        {
            string dataBuffer;

            Console.WriteLine("Device sending {0} messages to IoTHub...\n", MessageCount);

            for (int count = 0; count < MessageCount; count++)
            {
                _temperature = s_randomGenerator.Next(20, 35);
                _humidity = s_randomGenerator.Next(60, 80);
                dataBuffer = $"{{\"messageId\":{count},\"temperature\":{_temperature},\"humidity\":{_humidity}}}";
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                eventMessage.Properties.Add("temperatureAlert", (_temperature > TemperatureThreshold) ? "true" : "false");
                Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), count, dataBuffer);

                await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
            }
        }

        //private async Task ReceiveCommands()
        //{
        //    Console.WriteLine("\nDevice waiting for commands from IoTHub...\n");
        //    Console.WriteLine("Use the IoT Hub Azure Portal to send a message to this device.\n");

        //    Message receivedMessage;
        //    string messageData;

        //    receivedMessage = await _deviceClient.ReceiveAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

        //    if (receivedMessage != null)
        //    {
        //        messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
        //        Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

        //        int propCount = 0;
        //        foreach (var prop in receivedMessage.Properties)
        //        {
        //            Console.WriteLine("\t\tProperty[{0}> Key={1} : Value={2}", propCount++, prop.Key, prop.Value);
        //        }

        //        await _deviceClient.CompleteAsync(receivedMessage).ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        Console.WriteLine("\t{0}> Timed out", DateTime.Now.ToLocalTime());
        //    }
        //}

        private static async Task<Device> CreateDeviceAsync(string prefix, int index, string connectionString)
        {
            string deviceName = prefix + index + "_" + Guid.NewGuid();
            Device requestDevice = new Device(deviceName);

            using (RegistryManager rm = RegistryManager.CreateFromConnectionString(connectionString))
            {
                Console.WriteLine($"{nameof(CreateDeviceAsync)}: Creating device {deviceName}.");
                Device device = await rm.AddDeviceAsync(requestDevice).ConfigureAwait(false);

                requestDevice = device;

                await rm.CloseAsync().ConfigureAwait(false);
            }
            return requestDevice;
        }

        public static string GetHostName(string iotHubConnectionString)
        {
            Regex regex = new Regex("HostName=([^;]+)", RegexOptions.None);
            return regex.Match(iotHubConnectionString).Groups[1].Value;
        }
    }
}
