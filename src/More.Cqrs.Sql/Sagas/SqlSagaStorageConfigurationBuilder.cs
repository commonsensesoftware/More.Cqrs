// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Messaging;
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an object that can be used to build a <see cref="SqlSagaStorageConfiguration">SQL database saga store configuration</see>.
    /// </summary>
    public class SqlSagaStorageConfigurationBuilder
    {
        /// <summary>
        /// Gets the connection string used by the underlying database.
        /// </summary>
        /// <vaule>The connection string for the underlying database.</vaule>
        protected string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the identifier of the message saga table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the saga table.</vaule>
        protected SqlIdentifier TableName { get; private set; } = new SqlIdentifier( "Messaging", "Saga" );

        /// <summary>
        /// Gets the provider factory for the underlying database.
        /// </summary>
        /// <vaule>The <see cref="DbProviderFactory">provider factory</see> for the underlying database.</vaule>
        protected DbProviderFactory ProviderFactory { get; private set; } = SqlClientFactory.Instance;

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
        /// <returns>The original <see cref="SqlSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual SqlSagaStorageConfigurationBuilder HasConnectionString( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlSagaStorageConfigurationBuilder>() != null );

            ConnectionString = value;
            return this;
        }

        /// <summary>
        /// Applies the specified saga store table name to the configuration.
        /// </summary>
        /// <param name="schemaName">The configured saga store schema name. The default value is "Messaging".</param>
        /// <param name="tableName">The configured saga store table name. The default value is "Saga".</param>
        /// <returns>The original <see cref="SqlSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual SqlSagaStorageConfigurationBuilder HasTableName( string schemaName, string tableName )
        {
            Arg.NotNullOrEmpty( schemaName, nameof( schemaName ) );
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Contract.Ensures( Contract.Result<SqlSagaStorageConfigurationBuilder>() != null );

            TableName = new SqlIdentifier( schemaName, tableName );
            return this;
        }

        /// <summary>
        /// Applies the specified database provider factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="DbProviderFactory">database provider factory</see>.
        /// The default value is <see cref="SqlClientFactory"/>.</param>
        /// <returns>The original <see cref="SqlSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual SqlSagaStorageConfigurationBuilder UseProviderFactory( DbProviderFactory value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlSagaStorageConfigurationBuilder>() != null );

            ProviderFactory = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message type resolver to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageTypeResolver">message type resolver</see>.
        /// The default value is <see cref="DefaultMessageTypeResolver"/>.</param>
        /// <returns>The original <see cref="SqlSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual SqlSagaStorageConfigurationBuilder UseMessageTypeResolver( IMessageTypeResolver value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlSagaStorageConfigurationBuilder>() != null );

            MessageTypeResolver = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="ISqlMessageSerializerFactory">message serializer factory</see>.</param>
        /// <returns>The original <see cref="SqlSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual SqlSagaStorageConfigurationBuilder UseMessageSerializerFactory( ISqlMessageSerializerFactory value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlSagaStorageConfigurationBuilder>() != null );

            NewMessageSerializerFactory = _ => value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer to the configuration.
        /// </summary>
        /// <param name="value">The configured factory <see cref="Func{T,TResult}">method</see> used to create a new message serializer factory.</param>
        /// <returns>The original <see cref="SqlSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual SqlSagaStorageConfigurationBuilder UseMessageSerializerFactory( Func<IMessageTypeResolver, ISqlMessageSerializerFactory> value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlSagaStorageConfigurationBuilder>() != null );

            NewMessageSerializerFactory = value;
            return this;
        }

        /// <summary>
        /// Creates and returns a new SQL saga store configuration.
        /// </summary>
        /// <returns>A new <see cref="SqlSagaStorageConfiguration">SQL database saga store configuration</see>.</returns>
        public virtual SqlSagaStorageConfiguration CreateConfiguration()
        {
            Contract.Ensures( Contract.Result<SqlSagaStorageConfiguration>() != null );

            return new SqlSagaStorageConfiguration(
                ProviderFactory,
                ConnectionString,
                TableName,
                MessageTypeResolver,
                NewMessageSerializerFactory( MessageTypeResolver ) );
        }
    }
}