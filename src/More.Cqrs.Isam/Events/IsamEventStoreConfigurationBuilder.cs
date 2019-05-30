// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an object that can be used to build an <see cref="IsamEventStoreConfiguration">ISAM database event store configuration</see>.
    /// </summary>
    public class IsamEventStoreConfigurationBuilder
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
        /// <vaule>The identifier of the event store table.</vaule>
        protected string TableName { get; private set; } = "Events_Event";

        /// <summary>
        /// Gets the configuration aggregate snapshots.
        /// </summary>
        /// <value>The aggregate <see cref="IsamSnapshotConfiguration">snapshot configuration</see>.</value>
        protected IsamSnapshotConfiguration Snapshots { get; private set; } = new IsamSnapshotConfiguration( "Snapshots_Snapshot", supported: false );

        /// <summary>
        /// Gets the object used to resolve message types.
        /// </summary>
        /// <value>A <see cref="IMessageTypeResolver">message type resolver</see>.</value>
        protected IMessageTypeResolver MessageTypeResolver { get; private set; } = new DefaultMessageTypeResolver();

        /// <summary>
        /// Gets the factory method used to create factories for serialization and deserialization.
        /// </summary>
        /// <value>A <see cref="Func{T, TResult}">function</see> to create <see cref="IIsamMessageSerializerFactory">serializer factories</see>.</value>
        protected Func<IMessageTypeResolver, IIsamMessageSerializerFactory> NewMessageSerializerFactory { get; private set; } =
            messageTypeResolver => new IsamJsonMessageSerializerFactory( messageTypeResolver, JsonSettings.Default );

        /// <summary>
        /// Applies the specified entity name to the configuration.
        /// </summary>
        /// <param name="value">The configured entity name. The default value is "Event".</param>
        /// <returns>The original <see cref="IsamEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual IsamEventStoreConfigurationBuilder HasEntityName( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamEventStoreConfigurationBuilder>() != null );

            EntityName = value;

            if ( !TableNameIsOverriden )
            {
                TableName = value;
            }

            return this;
        }

        /// <summary>
        /// Applies the specified connection string to the configuration.
        /// </summary>
        /// <param name="value">The configured connection string.</param>
        /// <returns>The original <see cref="IsamEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual IsamEventStoreConfigurationBuilder HasConnectionString( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamEventStoreConfigurationBuilder>() != null );

            ConnectionString = value;
            return this;
        }

        /// <summary>
        /// Applies the specified event store table name to the configuration.
        /// </summary>
        /// <param name="tableName">The configured event store table name. The default value is based on the <see cref="EntityName"/> property.</param>
        /// <returns>The original <see cref="IsamEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual IsamEventStoreConfigurationBuilder HasTableName( string tableName )
        {
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Contract.Ensures( Contract.Result<IsamEventStoreConfigurationBuilder>() != null );

            TableName = tableName;
            TableNameIsOverriden = true;

            return this;
        }

        /// <summary>
        /// Applies the specified message type resolver to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageTypeResolver">message type resolver</see>.
        /// The default value is <see cref="DefaultMessageTypeResolver"/>.</param>
        /// <returns>The original <see cref="IsamEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual IsamEventStoreConfigurationBuilder UseMessageTypeResolver( IMessageTypeResolver value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamEventStoreConfigurationBuilder>() != null );

            MessageTypeResolver = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IIsamMessageSerializerFactory">message serializer factory</see>.</param>
        /// <returns>The original <see cref="IsamEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual IsamEventStoreConfigurationBuilder UseMessageSerializerFactory( IIsamMessageSerializerFactory value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamEventStoreConfigurationBuilder>() != null );

            NewMessageSerializerFactory = _ => value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer to the configuration.
        /// </summary>
        /// <param name="value">The configured factory <see cref="Func{T,TResult}">method</see> used to create a new message serializer factory.</param>
        /// <returns>The original <see cref="IsamEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual IsamEventStoreConfigurationBuilder UseMessageSerializerFactory( Func<IMessageTypeResolver, IIsamMessageSerializerFactory> value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamEventStoreConfigurationBuilder>() != null );

            NewMessageSerializerFactory = value;
            return this;
        }

        /// <summary>
        /// Enables aggregate snapshots for the event store configuration.
        /// </summary>
        /// <param name="tableName">The configured snapshot store table name. The default value is "Snapshots_Snapshot".</param>
        /// <returns>The original <see cref="IsamEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual IsamEventStoreConfigurationBuilder SupportsSnapshots( string tableName = "Snapshots_Snapshot" )
        {
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Contract.Ensures( Contract.Result<IsamEventStoreConfigurationBuilder>() != null );

            Snapshots = new IsamSnapshotConfiguration( tableName, supported: true );
            return this;
        }

        /// <summary>
        /// Creates and returns a new ISAM event store configuration.
        /// </summary>
        /// <returns>A new <see cref="IsamEventStoreConfiguration">ISAM database event store configuration</see>.</returns>
        public virtual IsamEventStoreConfiguration CreateConfiguration()
        {
            Contract.Ensures( Contract.Result<IsamEventStoreConfiguration>() != null );

            return new IsamEventStoreConfiguration(
                EntityName,
                ConnectionString,
                TableName,
                Snapshots,
                MessageTypeResolver,
                NewMessageSerializerFactory( MessageTypeResolver ) );
        }
    }
}