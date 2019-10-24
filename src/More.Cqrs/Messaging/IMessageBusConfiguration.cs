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
    /// Defines the behavior of a message bus configuration.
    /// </summary>
    public interface IMessageBusConfiguration : IServiceProvider
    {
        /// <summary>
        /// Gets the configured clock.
        /// </summary>
        /// <value>The configured <see cref="IClock">clock</see>.</value>
        IClock Clock { get; }

        /// <summary>
        /// Gets the mapping for persistence.
        /// </summary>
        /// <value>The <see cref="IMapPersistence">mapping for persistence</see>.</value>
        IMapPersistence Persistence { get; }

        /// <summary>
        /// Gets the configured message sender.
        /// </summary>
        /// <value>The configured <see cref="IMessageSender">message sender</see>.</value>
        IMessageSender MessageSender { get; }

        /// <summary>
        /// Gets the configured message receiver.
        /// </summary>
        /// <value>The configured <see cref="IMessageReceiver">message receiver</see>.</value>
        IMessageReceiver MessageReceiver { get; }

        /// <summary>
        /// Gets the configured event receiver registrar.
        /// </summary>
        /// <value>The configured <see cref="IEventReceiverRegistrar">event receiver registrar</see>.</value>
        IEventReceiverRegistrar EventReceivers { get; }

        /// <summary>
        /// Gets the configured command handler registrar.
        /// </summary>
        /// <value>The configured <see cref="ICommandHandlerRegistrar">command handler registrar</see>.</value>
        ICommandHandlerRegistrar CommandHandlers { get; }

        /// <summary>
        /// Gets the associated saga configuration.
        /// </summary>
        /// <value>The <see cref="SagaConfiguration">saga configuration</see> used by the message bus.</value>
        SagaConfiguration Sagas { get; }

        /// <summary>
        /// Gets the configured object used to generate unique identifiers.
        /// </summary>
        /// <returns>The configured <see cref="IUniqueIdGenerator">unique identifier generator</see>.</returns>
        IUniqueIdGenerator UniqueIdGenerator { get; }
    }
}