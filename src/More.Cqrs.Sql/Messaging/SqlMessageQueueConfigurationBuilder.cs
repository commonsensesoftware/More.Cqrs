// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;

    /// <summary>
    /// Represents an object that can be used to build a <see cref="SqlMessageQueueConfiguration">SQL database message queue configuration</see>.
    /// </summary>
    public class SqlMessageQueueConfigurationBuilder
    {
        /// <summary>
        /// Gets the connection string used by the underlying database.
        /// </summary>
        /// <vaule>The connection string for the underlying database.</vaule>
        protected string ConnectionString { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the identifier of the message queue table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the message queue table.</vaule>
        protected SqlIdentifier MessageQueueTableName { get; private set; } = new SqlIdentifier( "Messaging", "MessageQueue" );

        /// <summary>
        /// Gets the identifier of the subscription table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the subscription table.</vaule>
        protected SqlIdentifier SubscriptionTableName { get; private set; } = new SqlIdentifier( "Messaging", "Subscription" );

        /// <summary>
        /// Gets the identifier of the subscription queue table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the subscription queue table.</vaule>
        protected SqlIdentifier SubscriptionQueueTableName { get; private set; } = new SqlIdentifier( "Messaging", "SubscriptionQueue" );

        /// <summary>
        /// Gets the provider factory for the underlying database.
        /// </summary>
        /// <vaule>The <see cref="DbProviderFactory">provider factory</see> for the underlying database.</vaule>
        protected DbProviderFactory ProviderFactory { get; private set; } = SqlClientFactory.Instance;

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
        /// <vaule>The factory <see cref="Func{T, TResult}">method</see> used to create <see cref="ISqlMessageSerializerFactory">serializer factories</see>.</vaule>
        protected Func<IMessageTypeResolver, ISqlMessageSerializerFactory> NewMessageSerializerFactory { get; private set; } =
            messageTypeResolver => new SqlJsonMessageSerializerFactory( messageTypeResolver, JsonSettings.Default );

        /// <summary>
        /// Applies the specified connection string to the configuration.
        /// </summary>
        /// <param name="value">The configured connection string.</param>
        /// <returns>The original <see cref="SqlMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual SqlMessageQueueConfigurationBuilder HasConnectionString( string value )
        {
            ConnectionString = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message queue table name to the configuration.
        /// </summary>
        /// <param name="schemaName">The configured message queue schema name. The default value is "Messaging".</param>
        /// <param name="tableName">The configured message queue table name. The default value is "MessageQueue".</param>
        /// <returns>The original <see cref="SqlMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual SqlMessageQueueConfigurationBuilder HasMessageQueueTableName( string schemaName, string tableName )
        {
            MessageQueueTableName = new SqlIdentifier( schemaName, tableName );
            return this;
        }

        /// <summary>
        /// Applies the specified subscription table name to the configuration.
        /// </summary>
        /// <param name="schemaName">The configured message queue schema name. The default value is "Messaging".</param>
        /// <param name="tableName">The configured message queue table name. The default value is "Subscription".</param>
        /// <returns>The original <see cref="SqlMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual SqlMessageQueueConfigurationBuilder HasSubscriptionTableName( string schemaName, string tableName )
        {
            SubscriptionTableName = new SqlIdentifier( schemaName, tableName );
            return this;
        }

        /// <summary>
        /// Applies the specified subscription queue table name to the configuration.
        /// </summary>
        /// <param name="schemaName">The configured message queue schema name. The default value is "Messaging".</param>
        /// <param name="tableName">The configured message queue table name. The default value is "SubscriptionQueue".</param>
        /// <returns>The original <see cref="SqlMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual SqlMessageQueueConfigurationBuilder HasSubscriptionQueueTableName( string schemaName, string tableName )
        {
            SubscriptionQueueTableName = new SqlIdentifier( schemaName, tableName );
            return this;
        }

        /// <summary>
        /// Applies the specified database provider factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="DbProviderFactory">database provider factory</see>.
        /// The default value is <see cref="SqlClientFactory"/>.</param>
        /// <returns>The original <see cref="SqlMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual SqlMessageQueueConfigurationBuilder UseProviderFactory( DbProviderFactory value )
        {
            ProviderFactory = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message type resolver to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageTypeResolver">message type resolver</see>.
        /// The default value is <see cref="DefaultMessageTypeResolver"/>.</param>
        /// <returns>The original <see cref="SqlMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual SqlMessageQueueConfigurationBuilder UseMessageTypeResolver( IMessageTypeResolver value )
        {
            MessageTypeResolver = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="ISqlMessageSerializerFactory">message serializer factory</see>.</param>
        /// <returns>The original <see cref="SqlMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual SqlMessageQueueConfigurationBuilder UseMessageSerializerFactory( ISqlMessageSerializerFactory value )
        {
            NewMessageSerializerFactory = _ => value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer to the configuration.
        /// </summary>
        /// <param name="value">The configured factory <see cref="Func{T,TResult}">method</see> used to create a new message serializer factory.</param>
        /// <returns>The original <see cref="SqlMessageQueueConfigurationBuilder">builder</see>.</returns>
        public virtual SqlMessageQueueConfigurationBuilder UseMessageSerializerFactory( Func<IMessageTypeResolver, ISqlMessageSerializerFactory> value )
        {
            NewMessageSerializerFactory = value;
            return this;
        }

        /// <summary>
        /// Creates and returns a new SQL message queue configuration.
        /// </summary>
        /// <returns>A new <see cref="SqlMessageQueueConfiguration">SQL database message queue configuration</see>.</returns>
        public virtual SqlMessageQueueConfiguration CreateConfiguration()
        {
            return new SqlMessageQueueConfiguration(
                Clock,
                ProviderFactory,
                ConnectionString,
                MessageQueueTableName,
                SubscriptionTableName,
                SubscriptionQueueTableName,
                MessageTypeResolver,
                NewMessageSerializerFactory( MessageTypeResolver ) );
        }
    }
}