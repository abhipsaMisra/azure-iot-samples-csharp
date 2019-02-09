// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Common;
using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    // EventHubListener Platform Adaptation Layer for .NET Standard
    // This is using the new Microsoft.Azure.EventHubs from https://github.com/Azure/azure-event-hubs
    public partial class EventHubTestListener
    {
        private static string _eventHubConnectionString = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_CONN_STRING_CSHARP");
        private static string _eventHubCompatibleName = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_COMPATIBLE_NAME");
        private static string _eventHubConsumerName = Environment.GetEnvironmentVariable("IOTHUB_EVENTHUB_CONSUMER_GROUP");

        private PartitionReceiver _receiver;

        private EventHubTestListener(PartitionReceiver receiver)
        {
            _receiver = receiver;
        }

        public static async Task<EventHubTestListener> CreateListenerPal(string deviceName)
        {
            PartitionReceiver receiver = null;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var builder = new EventHubsConnectionStringBuilder(_eventHubConnectionString)
            {
                EntityPath = _eventHubCompatibleName
            };

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(builder.ToString());
            var eventRuntimeInformation = await eventHubClient.GetRuntimeInformationAsync().ConfigureAwait(false);
            var eventHubPartitionsCount = eventRuntimeInformation.PartitionCount;
            string partition = EventHubPartitionKeyResolver.ResolveToPartition(deviceName, eventHubPartitionsCount);
            string consumerGroupName = _eventHubConsumerName;

            while (receiver == null && sw.Elapsed.TotalMinutes < MaximumWaitTimeInMinutes)
            {
                try
                {
                    receiver = eventHubClient.CreateReceiver(consumerGroupName, partition, DateTime.Now.AddMinutes(-LookbackTimeInMinutes));
                }
                catch (EventHubsException ex)
                {
                    Console.WriteLine($"{nameof(EventHubTestListener)}.{nameof(CreateListener)}: Cannot create receiver: {ex}");
                }
            }

            sw.Stop();

            return new EventHubTestListener(receiver);
        }
    }
}
