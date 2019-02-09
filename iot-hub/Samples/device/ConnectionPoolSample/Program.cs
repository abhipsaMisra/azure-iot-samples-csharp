// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class Program
    {
        private static string s_iothubConnectionString = Environment.GetEnvironmentVariable("IOTHUB_CONN_STRING_CSHARP");
        private static string s_iothubDeviceConnectionString = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_LEVEL_CONN_STRING");
        private static string s_eventHubConnectionString = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_CONN_STRING_CSHARP");
        private static string s_eventHubCompatibleName = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_COMPATIBLE_NAME");
        private static string s_eventHubConsumerName = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_CONSUMER_GROUP");

        private static string s_devicePrefix = "ConnectionPoolingDevice";
        private static int s_maxDevices = 10;

        public static int Main(string[] args)
        {   
            if (string.IsNullOrWhiteSpace(s_iothubConnectionString)
                || string.IsNullOrWhiteSpace(s_iothubDeviceConnectionString)
                || string.IsNullOrWhiteSpace(s_eventHubConnectionString)
                || string.IsNullOrWhiteSpace(s_eventHubCompatibleName)
                || string.IsNullOrWhiteSpace(s_eventHubConsumerName))
            {
                Console.WriteLine("Please set all 5 vars");
                return 1;
            }

            for (int i=0; i<s_maxDevices; i++)
            {
                var sample = new ConnectionPoolSample($"{s_devicePrefix}_{i}");
                sample.RunSampleAsync().GetAwaiter().GetResult();
            }

            Console.WriteLine("Done.\n");
            Console.ReadLine();
            return 0;
        }
    }
}
