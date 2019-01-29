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

            var sample = new MessageSample(s_transportType, s_connectionString, s_multiplexingDeviceCount, DevicePrefix);
            sample.RunSampleAsync().GetAwaiter().GetResult();

            Console.WriteLine("Done.\n");
            return 0;
        }
    }
}
