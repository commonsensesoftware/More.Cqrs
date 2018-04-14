// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System;
    using System.Diagnostics.Contracts;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Represents an object that can construct a <see cref="IMessageBusConfiguration">message bus configuration</see>.
    /// </summary>
    public class MessageBusConfigurationBuilder
    {
        bool CommandRegistrarIsOverriden { get; set; }

        bool EventReceiverRegistrarIsOverriden { get; set; }

        /// <summary>
        /// Gets the object used to resolve services.
        /// </summary>
        /// <value>The <see cref="IServiceProvider">service provider</see> used to resolve services.</value>
        protected IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the clock used for scheduling messages.
        /// </summary>
        /// <value>The <see cref="IClock">clock</see> used to schedule messages.</value>
        protected IClock Clock { get; private set; }

        /// <summary>
        /// Gets the object used to map persistence.
        /// </summary>
        /// <value>The <see cref="IMapPersistence">persistence map</see>.</value>
        protected IMapPersistence Persistence { get; private set; }

        /// <summary>
        /// Gets the object used to send messages.
        /// </summary>
        /// <value>The <see cref="IMessageSender">message sender</see>.</value>
        protected IMessageSender MessageSender { get; private set; }

        /// <summary>
        /// Gets the object used to receive messages.
        /// </summary>
        /// <value>The <see cref="IMessageReceiver">message receiver</see>.</value>
        protected IMessageReceiver MessageReceiver { get; private set; }

        /// <summary>
        /// Gets the object used to register command handlers.
        /// </summary>
        /// <value>The <see cref="ICommandHandlerRegistrar">command handler registrar</see>.</value>
        protected ICommandHandlerRegistrar CommandRegistrar { get; private set; }

        /// <summary>
        /// Gets the object used to register event handlers.
        /// </summary>
        /// <value>The <see cref="IEventReceiverRegistrar">event handler registrar</see>.</value>
        protected IEventReceiverRegistrar EventRegistrar { get; private set; }

        /// <summary>
        /// Gets the object used to store the state of sagas.
        /// </summary>
        /// <value>The <see cref="IStoreSagaData">saga data storagage</see>.</value>
        protected IStoreSagaData SagaStorage { get; private set; }

        /// <summary>
        /// Gets the collection of metadata defined sagas.
        /// </summary>
        /// <value>A <see cref="SagaMetadataCollection">saga metadata collection</see>.</value>
        protected SagaMetadataCollection SagaMetadata { get; private set; }

        /// <summary>
        /// Gets the object used to create unique identifiers.
        /// </summary>
        /// <value>The <see cref="IUniqueIdGenerator">unique identifier generator</see>.</value>
        protected IUniqueIdGenerator UniqueIdGenerator { get; private set; }

        /// <summary>
        /// Indicates the message bus configuration will have the specified service provider.
        /// </summary>
        /// <param name="value">The configured <see cref="IServiceProvider">service provider</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder HasServiceProvider( IServiceProvider value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            ServiceProvider = value;

            if ( !CommandRegistrarIsOverriden )
            {
                CommandRegistrar = new CommandRegistrar( value );
            }

            if ( !EventReceiverRegistrarIsOverriden )
            {
                EventRegistrar = new EventRegistrar( value );
            }

            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will use the specified clock.
        /// </summary>
        /// <param name="value">The configured <see cref="IClock">clock</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder UseClock( IClock value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            Clock = value;
            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will have the specified persistence mapping.
        /// </summary>
        /// <param name="value">The configured <see cref="IMapPersistence">persistence mapping</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder MapPersistenceWith( IMapPersistence value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            Persistence = value;
            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will have the specified message sender.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageSender">message sender</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder HasMessageSender( IMessageSender value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            MessageSender = value;
            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will have the specified message receiver.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageReceiver">message receiver</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder HasMessageReceiver( IMessageReceiver value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            MessageReceiver = value;
            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will have the specified command handler registrar.
        /// </summary>
        /// <param name="value">The configured <see cref="ICommandHandlerRegistrar">command registrar</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder HasCommandHandlerRegistrar( ICommandHandlerRegistrar value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            CommandRegistrar = value;
            CommandRegistrarIsOverriden = true;
            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will have the specified event receiver registrar.
        /// </summary>
        /// <param name="value">The configured <see cref="IEventReceiverRegistrar">event registrar</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder HasEventReceiverRegistrar( IEventReceiverRegistrar value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            EventRegistrar = value;
            EventReceiverRegistrarIsOverriden = true;
            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will have the specified saga store.
        /// </summary>
        /// <param name="value">The configured <see cref="IStoreSagaData">saga store</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder HasSagaStorage( IStoreSagaData value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            SagaStorage = value;
            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will have the specified saga metadata.
        /// </summary>
        /// <param name="value">The configured <see cref="SagaMetadataCollection">saga metadata</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        public virtual MessageBusConfigurationBuilder HasSagaMetadata( SagaMetadataCollection value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            SagaMetadata = value;
            return this;
        }

        /// <summary>
        /// Indicates the message bus configuration will use the specified unique identifier generator.
        /// </summary>
        /// <param name="value">The configured <see cref="IUniqueIdGenerator">unique identifier generator</see>.</param>
        /// <returns>The original <see cref="MessageBusConfigurationBuilder"/> instance.</returns>
        /// <remarks>The default configuration uses <see cref="Guid.NewGuid"/>.</remarks>
        public virtual MessageBusConfigurationBuilder UseUniqueIdGenerator( IUniqueIdGenerator value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<MessageBusConfiguration>() != null );

            UniqueIdGenerator = value;
            return this;
        }

        /// <summary>
        /// Creates a new message bus configuration.
        /// </summary>
        /// <returns>A new <see cref="IMessageBusConfiguration">message bus configuration</see>.</returns>
        public virtual IMessageBusConfiguration CreateConfiguration()
        {
            Contract.Ensures( Contract.Result<IMessageBusConfiguration>() != null );

            var serviceProvider = ServiceProvider ?? Domain.ServiceProvider.Default;

            return new MessageBusConfiguration(
                serviceProvider,
                Clock ?? (IClock) serviceProvider.GetService( typeof( IClock ) ) ?? new SystemClock(),
                Persistence ?? (IMapPersistence) serviceProvider.GetService( typeof( IMapPersistence ) ) ?? new PersistenceMapper(),
                MessageSender ?? (IMessageSender) serviceProvider.GetService( typeof( IMessageSender ) ) ?? DefaultConfiguration.MessageSender,
                MessageReceiver ?? (IMessageReceiver) serviceProvider.GetService( typeof( IMessageReceiver ) ) ?? DefaultConfiguration.MessageReceiver,
                CommandRegistrar ?? (ICommandHandlerRegistrar) serviceProvider.GetService( typeof( ICommandHandlerRegistrar ) ) ?? new CommandRegistrar( serviceProvider ),
                EventRegistrar ?? (IEventReceiverRegistrar) serviceProvider.GetService( typeof( IEventReceiverRegistrar ) ) ?? new EventRegistrar( serviceProvider ),
                new SagaConfiguration(
                    SagaStorage ?? (IStoreSagaData) serviceProvider.GetService( typeof( IStoreSagaData ) ) ?? DefaultConfiguration.SagaStorage,
                    SagaMetadata ?? (SagaMetadataCollection) serviceProvider.GetService( typeof( SagaMetadataCollection ) ) ?? new SagaMetadataCollection( Empty<Type>() ) ),
                UniqueIdGenerator ?? (IUniqueIdGenerator) serviceProvider.GetService( typeof( IUniqueIdGenerator ) ) ?? new DefaultUniqueIdGenerator() );
        }
    }
}