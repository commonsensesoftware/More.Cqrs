// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Microsoft Service Bus configuration.
    /// </summary>
    public class ServiceBusConfiguration
    {
        /// <summary>
        /// Gets the receive options.
        /// </summary>
        /// <value>The <see cref="OnMessageOptions">receive options</see>.</value>
        public OnMessageOptions ReceiveOptions { get; }

        /// <summary>
        /// Gets the object used to resolve message types.
        /// </summary>
        /// <value>A <see cref="IMessageTypeResolver">message type resolver</see>.</value>
        public IMessageTypeResolver MessageTypeResolver { get; }

        /// <summary>
        /// Gets the or sets the JSON seralizer.
        /// </summary>
        /// <value>The <see cref="JsonSerializer">serializer</see> for JSON.</value>
        public JsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Creates a new topic client.
        /// </summary>
        /// <returns>A new <see cref="TopicClient">topic client</see>.</returns>
        public virtual TopicClient CreateTopicClient()
        {
            return null;
        }

        /// <summary>
        /// Creates a new subscription client.
        /// </summary>
        /// <returns>A new <see cref="SubscriptionClient">subscription client</see>.</returns>
        public virtual SubscriptionClient CreateSubscriptionClient()
        {
            return null;
        }

        /// <summary>
        /// Creates a new message sender.
        /// </summary>
        /// <returns>A new <see cref="MessageSender">message sender</see>.</returns>
        public virtual MessageSender CreateSender()
        {
            return null;
        }

        /// <summary>
        /// Creates a new message receiver.
        /// </summary>
        /// <returns>A new <see cref="MessageReceiver">message receiver</see>.</returns>
        public virtual MessageReceiver CreateReceiver()
        {
            return null;
        }
    }
}