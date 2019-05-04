// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class Program
    {
        // Event Hub configuration details can be found at: Azure --> IotHub --> Built-in endpoints
        // IOTHUB_EVENTHUB_CONN_STRING = Event Hub-compatible endpoint
        // IOTHUB_EVENTHUB_COMPATIBLE_NAME = Event Hub-compatible name
        // IOTHUB_EVENTHUB_COMPATIBLE_NAME = Consumer group (default is $Default)

        // For this sample either
        // - pass this value as a command-prompt argument
        // - set the IOTHUB_EVENTHUB_CONN_STRING environment variable 
        // - create a launchSettings.json (see launchSettings.json.template) containing the variable
        private static string s_eventHubConnectionString = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_CONN_STRING");
        private static string s_eventHubCompatibleName = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_COMPATIBLE_NAME");
        private static string s_eventHubConsumerGroup = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_CONSUMER_GROUP");
        private static string s_deviceId = Environment.GetEnvironmentVariable("DEVICE_ID");

        public static int Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(s_eventHubConnectionString))
            {
                if (args.Length > 3)
                {
                    s_eventHubConnectionString = args[0];
                    s_eventHubCompatibleName = args[1];
                    s_eventHubConsumerGroup = args[2];
                    s_deviceId = args[3];
                }
                else
                {
                    Console.WriteLine("set the following args in environment or pass them ordered as below:");
                    Console.WriteLine("<IOTHUB_EVENTHUB_CONN_STRING> <IOTHUB_EVENTHUB_COMPATIBLE_NAME> <IOTHUB_EVENTHUB_CONSUMER_GROUP> <DEVICE_ID>");
                    return 1;
                }
            }

            var sample = new EventHubSample(s_eventHubConnectionString, s_eventHubCompatibleName, s_eventHubConsumerGroup, s_deviceId);
            sample.RunSampleAsync().GetAwaiter().GetResult();

            Console.WriteLine("Done. Press any key to end!");
            Console.ReadLine();
            return 0;
        }
    }
}
