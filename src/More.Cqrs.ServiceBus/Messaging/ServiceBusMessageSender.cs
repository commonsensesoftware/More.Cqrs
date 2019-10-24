// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Microsoft.ServiceBus.Messaging;
    using More.Domain.Options;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Text.Encoding;

    /// <summary>
    /// Represents a <see cref="IMessageSender">message sender</see> that is backed by Microsoft Service Bus.
    /// </summary>
    public class ServiceBusMessageSender : IMessageSender
    {
        readonly TopicClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusMessageSender"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ServiceBusConfiguration">Service Bus configuration</see> used by the message sender.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> used when messages that require scheduling.</param>
        public ServiceBusMessageSender( ServiceBusConfiguration configuration, IClock clock )
        {
            client = configuration.CreateTopicClient();
            Configuration = configuration;
            Clock = clock;
        }

        /// <summary>
        /// Gets the current Service Bus configuration.
        /// </summary>
        /// <value>The <see cref="ServiceBusConfiguration">Service Bus configuration</see> used by the message sender.</value>
        protected ServiceBusConfiguration Configuration { get; }

        /// <summary>
        /// Gets the clock used when messages that require scheduling.
        /// </summary>
        /// <value>The <see cref="IClock">clock</see> used for temporal messaging.</value>
        protected IClock Clock { get; }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="messages">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">messages</see> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task Send( IEnumerable<IMessageDescriptor> messages, CancellationToken cancellationToken )
        {
            foreach ( var message in messages )
            {
                var deliveryTime = message.Options.GetDeliveryTime( Clock );

                using ( var brokeredMessage = ToBrokeredMessage( message ) )
                {
                    if ( deliveryTime > Clock.Now )
                    {
                        await client.ScheduleMessageAsync( brokeredMessage, deliveryTime ).ConfigureAwait( false );
                    }
                    else
                    {
                        await client.SendAsync( brokeredMessage ).ConfigureAwait( false );
                    }
                }
            }
        }

        /// <summary>
        /// Creates and returns a new brokered message based on the specified message descriptor.
        /// </summary>
        /// <param name="messageDescriptor">The <see cref="IMessageDescriptor">message descriptor</see> to prepare.</param>
        /// <returns>A new, prepared <see cref="BrokeredMessage">message</see>.</returns>
        protected virtual BrokeredMessage ToBrokeredMessage( IMessageDescriptor messageDescriptor )
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter( stream, UTF8 );

            Configuration.JsonSerializer.Serialize( writer, messageDescriptor.Message );
            writer.Flush();
            stream.Position = 0L;

            var brokeredMessage = new BrokeredMessage( stream, ownsStream: true )
            {
                MessageId = messageDescriptor.MessageId,
                ContentType = "application/json",
                CorrelationId = messageDescriptor.Message.CorrelationId,
                Label = messageDescriptor.Message.GetType().Name,
                Properties =
                {
                    ["Type"] = messageDescriptor.MessageType,
                    ["Revision"] = messageDescriptor.Message.Revision,
                },
            };

            if ( messageDescriptor.AggregateId != null )
            {
                brokeredMessage.Properties["AggregateId"] = messageDescriptor.AggregateId;
            }

            return brokeredMessage;
        }
    }
}