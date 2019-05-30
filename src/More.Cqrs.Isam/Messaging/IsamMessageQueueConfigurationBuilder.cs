// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an object that can be used to build an <see cref="IsamMessageQueueConfiguration">ISAM database message queue configuration</see>.
    /// </summary>
    public class IsamMessageQueueConfigurationBuilder
    {
        /// <summary>
        /// Gets the connection string used by the underlying database.
        /// </summary>
        /// <vaule>The connection string for the underlying database.</vaule>
        protected string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the identifier of the message queue table.
        /// </summary>
        /// <vaule>The identifier of the message queue table.</vaule>
        protected string MessageQueueTableName { get; private set; } = "Messaging_MessageQueue";

        /// <summary>
        /// Gets the identifier of the subscription table.
        /// </summary>
        /// <vaule>The identifier of the subscription table.</vaule>
        protected string SubscriptionTableName { get; private set; } = "Messaging_Subscription";

        /// <summary>
        /// Gets the identifier of the subscription queue table.
        /// </summary>
        /// <vaule>The identifier of the subscription queue table.</vaule>
        protected string SubscriptionQueueTableName { get; private set; } = "Messaging_SubscriptionQueue";

        /// <summary>
        /// Gets the clock used to schedule message delivery.
        /// </summary>
        /// <vaule>The <see cref="IClock">clock</see> used to schedule messages.</vaule>
        protected IClock Clock { get; private set; } = new SystemClock();

        /// <summary>
        /// Gets the object used to resolve message types.
        /// </summary>
        /// <vaule>The <see cref="IMessageTypeResolver">message type resolver</see>.</vaule>
        protected IMessageTypeResolver MessageTypeResolver { get; private set; } = new DefaultMessageTypeResolver();

        /// <summary>
        /// Gets the factory method used to generate serializer factories.
        /// </summary>
        /// <vaule>The factory <see cref="Func{T, TResult}">method</see> used to create <see cref="IIsamMessageSerializerFactory">serializer factories</see>.</vaule>
        protected Func<IMessageTypeResolver, IIsamMessageSerializerFactory> NewMessageSerializerFactory { get; private set; } =
            messageTypeResolver => new IsamJsonMessageSerializerFactory( messageTypeResolver, JsonSettings.Default );

        /// <summary>
        /// Applies the specified connection string to the configuration.
        /// </summary>
        /// <param name="value">The configured connection string.</param>
        /// <returns>The original <see cref="IsamMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual IsamMessageQueueConfigurationBuilder HasConnectionString( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamMessageQueueConfigurationBuilder>() != null );

            ConnectionString = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message queue table name to the configuration.
        /// </summary>
        /// <param name="tableName">The configured message queue table name. The default value is "Messaging_MessageQueue".</param>
        /// <returns>The original <see cref="IsamMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual IsamMessageQueueConfigurationBuilder HasMessageQueueTableName( string tableName )
        {
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Contract.Ensures( Contract.Result<IsamMessageQueueConfigurationBuilder>() != null );

            MessageQueueTableName = tableName;
            return this;
        }

        /// <summary>
        /// Applies the specified subscription table name to the configuration.
        /// </summary>
        /// <param name="tableName">The configured message queue table name. The default value is "Messaging_Subscription".</param>
        /// <returns>The original <see cref="IsamMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual IsamMessageQueueConfigurationBuilder HasSubscriptionTableName( string tableName )
        {
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Contract.Ensures( Contract.Result<IsamMessageQueueConfigurationBuilder>() != null );

            SubscriptionTableName = tableName;
            return this;
        }

        /// <summary>
        /// Applies the specified subscription queue table name to the configuration.
        /// </summary>
        /// <param name="tableName">The configured message queue table name. The default value is "Messaging_SubscriptionQueue".</param>
        /// <returns>The original <see cref="IsamMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual IsamMessageQueueConfigurationBuilder HasSubscriptionQueueTableName( string tableName )
        {
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Contract.Ensures( Contract.Result<IsamMessageQueueConfigurationBuilder>() != null );

            SubscriptionQueueTableName = tableName;
            return this;
        }

        /// <summary>
        /// Applies the specified message type resolver to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageTypeResolver">message type resolver</see>.
        /// The default value is <see cref="DefaultMessageTypeResolver"/>.</param>
        /// <returns>The original <see cref="IsamMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual IsamMessageQueueConfigurationBuilder UseMessageTypeResolver( IMessageTypeResolver value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamMessageQueueConfigurationBuilder>() != null );

            MessageTypeResolver = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IIsamMessageSerializerFactory">message serializer factory</see>.</param>
        /// <returns>The original <see cref="IsamMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual IsamMessageQueueConfigurationBuilder UseMessageSerializerFactory( IIsamMessageSerializerFactory value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamMessageQueueConfigurationBuilder>() != null );

            NewMessageSerializerFactory = _ => value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer to the configuration.
        /// </summary>
        /// <param name="value">The configured factory <see cref="Func{T,TResult}">method</see> used to create a new message serializer factory.</param>
        /// <returns>The original <see cref="IsamMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual IsamMessageQueueConfigurationBuilder UseMessageSerializerFactory( Func<IMessageTypeResolver, IIsamMessageSerializerFactory> value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamMessageQueueConfigurationBuilder>() != null );

            NewMessageSerializerFactory = value;
            return this;
        }

        /// <summary>
        /// Creates and returns a new ISAM message queue configuration.
        /// </summary>
        /// <returns>A new <see cref="IsamMessageQueueConfiguration">ISAM database message queue configuration</see>.</returns>
        public virtual IsamMessageQueueConfiguration CreateConfiguration()
        {
            Contract.Ensures( Contract.Result<IsamMessageQueueConfiguration>() != null );

            return new IsamMessageQueueConfiguration(
                Clock,
                ConnectionString,
                MessageQueueTableName,
                SubscriptionTableName,
                SubscriptionQueueTableName,
                MessageTypeResolver,
                NewMessageSerializerFactory( MessageTypeResolver ) );
        }
    }
}