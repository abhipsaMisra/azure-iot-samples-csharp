// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class Program
    {
        // String containing Hostname, Device Id & Device Key in one of the following formats:
        //  "HostName=<iothub_host_name>;SharedAccessKeyName=<shared_access_policy>;SharedAccessKey=<device_key>"

        // For this sample either
        // - pass this value as a command-prompt argument
        // - set the IOTHUB_CONN_STRING_CSHARP environment variable 
        // - create a launchSettings.json (see launchSettings.json.template) containing the variable
        private static string s_connectionString = Environment.GetEnvironmentVariable("IOTHUB_CONN_STRING_CSHARP");

        // Select one of the following transports used by DeviceClient to connect to IoT Hub.
        private static TransportType s_transportType = TransportType.Amqp_Tcp_Only;
        //private static TransportType s_transportType = TransportType.Amqp_WebSocket_Only;

        private static string DevicePrefix = "MultiplexingDevice_";
        private static int s_multiplexingDeviceCount = 100;

        private const int MessageCount = 5;
        private const int TemperatureThreshold = 30;
        private static Random s_randomGenerator = new Random();
        private float _temperature;
        private float _humidity;

        public static int Main(string[] args)
        {
            if (string.IsNullOrEmpty(s_connectionString) && args.Length > 0)
            {
                s_connectionString = args[0];
            }

            DeviceClient[] pool = CreateDeviceClientOverMultiplex(s_transportType, s_connectionString, s_multiplexingDeviceCount, DevicePrefix).GetAwaiter().GetResult();

            var sample = new MessageSample(pool);
            sample.RunSampleAsync().GetAwaiter().GetResult();

            Console.WriteLine("Done.\n");
            return 0;
        }

        private static async Task<DeviceClient[]> CreateDeviceClientOverMultiplex(TransportType transportType, string connectionString, int deviceCount, string prefix)
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
