// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class MessageSample
    {
        private DeviceClient[] _deviceClientPool;
        private Device[] _devicePool;
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
            _deviceClientPool = new DeviceClient[s_deviceCount];
            _devicePool = new Device[s_deviceCount];
        }

        public async Task RunSampleAsync()
        {
            for (int i = 0; i < s_deviceCount; i++)
            {
                _devicePool[i] = await CreateDeviceAsync(s_prefix, i, s_connectionString).ConfigureAwait(false);
                var auth = new DeviceAuthenticationWithRegistrySymmetricKey(_devicePool[i].Id, _devicePool[i].Authentication.SymmetricKey.PrimaryKey);

                _deviceClientPool[i] = DeviceClient.Create(
                    GetHostName(s_connectionString),
                    auth,
                    new ITransportSettings[]
                    {
                        new AmqpTransportSettings(s_transportType)
                        {
                            AmqpConnectionPoolSettings = new AmqpConnectionPoolSettings()
                            {
                                Pooling = true,
                                MaxPoolSize = 10
                            }
                        }
                    });
                Console.WriteLine($"Device with ID {_devicePool[i].Id} is created.");
                ExecuteOperation(_deviceClientPool[i]);
            }
        }

        private static async Task ExecuteOperation(DeviceClient deviceClient)
        {
            while (true)
            {
                Console.WriteLine("Sending Event...");
                await deviceClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))).ConfigureAwait(false);
                Console.WriteLine("Completed.");
            }
        }

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
