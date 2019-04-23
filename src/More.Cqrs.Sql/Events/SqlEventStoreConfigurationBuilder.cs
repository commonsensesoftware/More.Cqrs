// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an object that can be used to build a <see cref="SqlEventStoreConfiguration">SQL database event store configuration</see>.
    /// </summary>
    public class SqlEventStoreConfigurationBuilder
    {
        bool TableNameIsOverriden { get; set; }

        /// <summary>
        /// Gets the logical name of the entity that the event store is for.
        /// </summary>
        /// <value>The logical name of the corresponding entity the event store is for. The default value is "Entity".</value>
        protected string EntityName { get; private set; } = "Entity";

        /// <summary>
        /// Gets the connection string for the underlying database.
        /// </summary>
        /// <value>The underlying database connection string.</value>
        protected string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the identifier of the event store table.
        /// </summary>
        /// <vaule>The <see cref="SqlIdentifier">identifier</see> of the event store table.</vaule>
        protected SqlIdentifier TableName { get; private set; } = new SqlIdentifier( "Events", "Event" );

        /// <summary>
        /// Gets the factory for the backing database provider.
        /// </summary>
        /// <value>The <see cref="DbProviderFactory">provider factory</see> for the underlying database.</value>
        protected DbProviderFactory ProviderFactory { get; private set; } = SqlClientFactory.Instance;

        /// <summary>
        /// Gets the configuration aggregate snapshots.
        /// </summary>
        /// <value>The aggregate <see cref="SqlSnapshotConfiguration">snapshot configuration</see>.</value>
        protected SqlSnapshotConfiguration Snapshots { get; private set; } =
            new SqlSnapshotConfiguration( new SqlIdentifier( "Snapshots", "Snapshot" ), supported: false );

        /// <summary>
        /// Gets the object used to resolve message types.
        /// </summary>
        /// <value>A <see cref="IMessageTypeResolver">message type resolver</see>.</value>
        protected IMessageTypeResolver MessageTypeResolver { get; private set; } = new DefaultMessageTypeResolver();

        /// <summary>
        /// Gets the factory method used to create factories for serialization and deserialization.
        /// </summary>
        /// <value>A <see cref="Func{T, TResult}">function</see> to create <see cref="ISqlMessageSerializerFactory">serializer factories</see>.</value>
        protected Func<IMessageTypeResolver, ISqlMessageSerializerFactory> NewMessageSerializerFactory { get; private set; } =
            messageTypeResolver => new SqlJsonMessageSerializerFactory( messageTypeResolver, JsonSettings.Default );

        /// <summary>
        /// Applies the specified entity name to the configuration.
        /// </summary>
        /// <param name="value">The configured entity name. The default value is "Event".</param>
        /// <returns>The original <see cref="SqlEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual SqlEventStoreConfigurationBuilder HasEntityName( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlEventStoreConfigurationBuilder>() != null );

            EntityName = value;

            if ( !TableNameIsOverriden )
            {
                TableName = new SqlIdentifier( TableName.SchemaName, value );
            }

            return this;
        }

        /// <summary>
        /// Applies the specified connection string to the configuration.
        /// </summary>
        /// <param name="value">The configured connection string.</param>
        /// <returns>The original <see cref="SqlEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual SqlEventStoreConfigurationBuilder HasConnectionString( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlEventStoreConfigurationBuilder>() != null );

            ConnectionString = value;
            return this;
        }

        /// <summary>
        /// Applies the specified event store table name to the configuration.
        /// </summary>
        /// <param name="schemaName">The configured event store schema name. The default value is "Events".</param>
        /// <param name="tableName">The configured event store table name. The default value is based on the <see cref="EntityName"/> property.</param>
        /// <returns>The original <see cref="SqlEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual SqlEventStoreConfigurationBuilder HasTableName( string schemaName, string tableName )
        {
            Arg.NotNullOrEmpty( schemaName, nameof( schemaName ) );
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Contract.Ensures( Contract.Result<SqlEventStoreConfigurationBuilder>() != null );

            TableNameIsOverriden = tableName != TableName.ObjectName;
            TableName = new SqlIdentifier( schemaName, tableName );
            return this;
        }

        /// <summary>
        /// Applies the specified database provider factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="DbProviderFactory">database provider factory</see>.
        /// The default value is <see cref="SqlClientFactory"/>.</param>
        /// <returns>The original <see cref="SqlEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual SqlEventStoreConfigurationBuilder UseProviderFactory( DbProviderFactory value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlEventStoreConfigurationBuilder>() != null );

            ProviderFactory = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message type resolver to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageTypeResolver">message type resolver</see>.
        /// The default value is <see cref="DefaultMessageTypeResolver"/>.</param>
        /// <returns>The original <see cref="SqlEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual SqlEventStoreConfigurationBuilder UseMessageTypeResolver( IMessageTypeResolver value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlEventStoreConfigurationBuilder>() != null );

            MessageTypeResolver = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="ISqlMessageSerializerFactory">message serializer factory</see>.</param>
        /// <returns>The original <see cref="SqlEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual SqlEventStoreConfigurationBuilder UseMessageSerializerFactory( ISqlMessageSerializerFactory value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlEventStoreConfigurationBuilder>() != null );

            NewMessageSerializerFactory = _ => value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer to the configuration.
        /// </summary>
        /// <param name="value">The configured factory <see cref="Func{T,TResult}">method</see> used to create a new message serializer factory.</param>
        /// <returns>The original <see cref="SqlEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual SqlEventStoreConfigurationBuilder UseMessageSerializerFactory( Func<IMessageTypeResolver, ISqlMessageSerializerFactory> value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<SqlEventStoreConfigurationBuilder>() != null );

            NewMessageSerializerFactory = value;
            return this;
        }

        /// <summary>
        /// Enables aggregate snapshots for the event store configuration.
        /// </summary>
        /// <param name="tableName">The configured snapshot store table name. The default value is "Snapshot".</param>
        /// <param name="schemaName">The configured snapshot store schema name. The default value is "Snapshots".</param>
        /// <returns>The original <see cref="SqlEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual SqlEventStoreConfigurationBuilder SupportsSnapshots( string tableName = "Snapshot", string schemaName = "Snapshots" )
        {
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Arg.NotNullOrEmpty( schemaName, nameof( schemaName ) );
            Contract.Ensures( Contract.Result<SqlEventStoreConfigurationBuilder>() != null );

            Snapshots = new SqlSnapshotConfiguration( new SqlIdentifier( schemaName, tableName ), supported: true );
            return this;
        }

        /// <summary>
        /// Creates and returns a new SQL event store configuration.
        /// </summary>
        /// <returns>A new <see cref="SqlEventStoreConfiguration">SQL database event store configuration</see>.</returns>
        public virtual SqlEventStoreConfiguration CreateConfiguration()
        {
            Contract.Ensures( Contract.Result<SqlEventStoreConfiguration>() != null );

            return new SqlEventStoreConfiguration(
                EntityName,
                ProviderFactory,
                ConnectionString,
                TableName,
                Snapshots,
                MessageTypeResolver,
                NewMessageSerializerFactory( MessageTypeResolver ) );
        }
    }
}