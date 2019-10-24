// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Persistence;
    using System;

    /// <summary>
    /// Represents a message bus configuration.
    /// </summary>
    public class MessageBusConfiguration : IMessageBusConfiguration
    {
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBusConfiguration"/> class.
        /// </summary>
        /// <param name="serviceProvider">The configured <see cref="IServiceProvider">service provider</see>.</param>
        /// <param name="clock">The configured <see cref="IClock">clock</see>.</param>
        /// <param name="persistence">The <see cref="IMapPersistence">mapping for persistence</see>.</param>
        /// <param name="messageSender">The configured <see cref="IMessageSender">message sender</see>.</param>
        /// <param name="messageReceiver">The configured <see cref="IMessageReceiver">message receiver</see>.</param>
        /// <param name="commandHandlerRegistrar">The configured <see cref="ICommandHandlerRegistrar">command handler registrar</see>.</param>
        /// <param name="eventReceiverRegistrar">The configured <see cref="IEventReceiverRegistrar">event handler registrar</see>.</param>
        /// <param name="sagaConfiguration">The <see cref="SagaConfiguration">saga configuration</see> used by the message bus.</param>
        /// <param name="uniqueIdGenerator">The configured <see cref="IUniqueIdGenerator">unique identifier generator</see> used by the message bus.</param>
        public MessageBusConfiguration(
            IServiceProvider serviceProvider,
            IClock clock,
            IMapPersistence persistence,
            IMessageSender messageSender,
            IMessageReceiver messageReceiver,
            ICommandHandlerRegistrar commandHandlerRegistrar,
            IEventReceiverRegistrar eventReceiverRegistrar,
            SagaConfiguration sagaConfiguration,
            IUniqueIdGenerator uniqueIdGenerator )
        {
            this.serviceProvider = serviceProvider;
            Clock = clock;
            Persistence = persistence;
            MessageSender = messageSender;
            MessageReceiver = messageReceiver;
            CommandHandlers = commandHandlerRegistrar;
            EventReceivers = eventReceiverRegistrar;
            Sagas = sagaConfiguration;
            UniqueIdGenerator = uniqueIdGenerator;
        }

        /// <summary>
        /// Gets the configured clock.
        /// </summary>
        /// <value>The configured <see cref="IClock">clock</see>.</value>
        public IClock Clock { get; }

        /// <summary>
        /// Gets the mapping for persistence.
        /// </summary>
        /// <value>The <see cref="IMapPersistence">mapping for persistence</see>.</value>
        public IMapPersistence Persistence { get; }

        /// <summary>
        /// Gets the configured message sender.
        /// </summary>
        /// <value>The configured <see cref="IMessageSender">message sender</see>.</value>
        public IMessageSender MessageSender { get; }

        /// <summary>
        /// Gets the configured message receiver.
        /// </summary>
        /// <value>The configured <see cref="IMessageReceiver">message receiver</see>.</value>
        public IMessageReceiver MessageReceiver { get; }

        /// <summary>
        /// Gets the configured event receiver registrar.
        /// </summary>
        /// <value>The configured <see cref="IEventReceiverRegistrar">event handler registrar</see>.</value>
        public IEventReceiverRegistrar EventReceivers { get; }

        /// <summary>
        /// Gets the configured command handler registrar.
        /// </summary>
        /// <value>The configured <see cref="ICommandHandlerRegistrar">command handler registrar</see>.</value>
        public ICommandHandlerRegistrar CommandHandlers { get; }

        /// <summary>
        /// Gets the associated saga configuration.
        /// </summary>
        /// <value>The <see cref="SagaConfiguration">saga configuration</see> used by the message bus.</value>
        public SagaConfiguration Sagas { get; }

        /// <summary>
        /// Gets the configured object used to generate unique identifiers.
        /// </summary>
        /// <returns>The configured <see cref="IUniqueIdGenerator">unique identifier generator</see>.</returns>
        public IUniqueIdGenerator UniqueIdGenerator { get; }

        /// <summary>
        /// Gets a configured service matching the requested type.
        /// </summary>
        /// <param name="serviceType">The type of service requested.</param>
        /// <returns>The requested service or <c>null</c>.</returns>
        public object GetService( Type serviceType ) => serviceProvider.GetService( serviceType );
    }
}