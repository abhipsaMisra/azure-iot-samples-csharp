// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
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

        public MessageSample(DeviceClient[] deviceClientPool)
        {
            _deviceClientPool = deviceClientPool ?? throw new ArgumentNullException(nameof(deviceClientPool));
        }

        public async Task RunSampleAsync()
        {
            foreach (DeviceClient deviceClient in _deviceClientPool)
            {
                await SendEvent(deviceClient).ConfigureAwait(false);
            }
            //await ReceiveCommands().ConfigureAwait(false);
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
    }
}
