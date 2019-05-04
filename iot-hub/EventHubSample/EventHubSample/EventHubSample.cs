// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class EventHubSample
    {
        private const int MaximumWaitTimeInMinutes = 5;
        // the DateTime instant that receive operations will start receive events from.
        private const int LookbackTimeInMinutes = 1440; // receive events from the last 24hrs.
        private const int OperationTimeoutInSeconds = 10;

        private static string _eventConnectionString;
        private static string _eventCompatibleName;
        private static string _eventConsumerGroup;
        private static string _deviceName;

        public EventHubSample(string eventConnectionString, string eventCompatibleName, string consumerGroup, string deviceName)
        {
            _eventConnectionString = eventConnectionString;
            _eventCompatibleName = eventCompatibleName;
            _eventConsumerGroup = consumerGroup;
            _deviceName = deviceName;
        }

        public async Task RunSampleAsync()
        {
            PartitionReceiver receiver = await CreateEventHubListener().ConfigureAwait(false);
            await ReceiveAllMessages(receiver).ConfigureAwait(false);
        }

        private async Task<PartitionReceiver> CreateEventHubListener()
        {
            PartitionReceiver receiver = null;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var builder = new EventHubsConnectionStringBuilder(_eventConnectionString)
            {
                EntityPath = _eventCompatibleName
            };

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(builder.ToString());
            var eventRuntimeInformation = await eventHubClient.GetRuntimeInformationAsync().ConfigureAwait(false);
            var eventHubPartitionsCount = eventRuntimeInformation.PartitionCount;
            string partition = EventHubPartitionKeyResolver.ResolveToPartition(_deviceName, eventHubPartitionsCount);
            string consumerGroupName = _eventConsumerGroup;

            while (receiver == null && sw.Elapsed.TotalMinutes < MaximumWaitTimeInMinutes)
            {
                try
                {
                    receiver = eventHubClient.CreateReceiver(consumerGroupName, partition, DateTime.Now.AddMinutes(-LookbackTimeInMinutes));
                }
                catch (EventHubsException ex)
                {
                    Console.WriteLine($"Cannot create receiver: {ex}");
                }
            }

            sw.Stop();

            return receiver;
        }

        private async Task ReceiveAllMessages(PartitionReceiver receiver)
        {
            IEnumerable<EventData> events = await receiver.ReceiveAsync(int.MaxValue, TimeSpan.FromSeconds(OperationTimeoutInSeconds)).ConfigureAwait(false);

            while (events != null)
            {
                Console.WriteLine($"\n\n {nameof(EventHubSample)}.{nameof(ReceiveAllMessages)}: {events.Count()} events received.");

                var n = 1;
                foreach (var eventData in events)
                {
                    try
                    {
                        string data = Encoding.UTF8.GetString(eventData.Body.ToArray());

                        IDictionary<string, object> properties = eventData.Properties;
                        if (properties != null)
                        {
                            Console.WriteLine($"[Message_Test]Properties {n}:");
                            foreach (var property in properties)
                            {
                                Console.WriteLine($"[Message_Test]{property.Key}:{property.Value}");
                            }
                        }

                        Console.WriteLine($"[Message_Test]Payload: {data}");
                        n++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{nameof(EventHubSample)}.{nameof(ReceiveAllMessages)}: Cannot read eventData: {ex}");
                    }
                }

                events = await receiver.ReceiveAsync(int.MaxValue, TimeSpan.FromSeconds(OperationTimeoutInSeconds)).ConfigureAwait(false);
            }

            Console.WriteLine($"{nameof(EventHubSample)}.{nameof(ReceiveAllMessages)}: No more events left to be received.");
        }
    }
}
