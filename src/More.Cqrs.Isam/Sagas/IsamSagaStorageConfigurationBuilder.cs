// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Messaging;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an object that can be used to build an <see cref="IsamSagaStorageConfiguration">ISAM database saga store configuration</see>.
    /// </summary>
    public class IsamSagaStorageConfigurationBuilder
    {
        /// <summary>
        /// Gets the connection string used by the underlying database.
        /// </summary>
        /// <vaule>The connection string for the underlying database.</vaule>
        protected string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the identifier of the message saga table.
        /// </summary>
        /// <vaule>The identifier of the saga table.</vaule>
        protected string TableName { get; private set; } = "Messaging_Saga";

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
        /// <returns>The original <see cref="IsamSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual IsamSagaStorageConfigurationBuilder HasConnectionString( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamSagaStorageConfigurationBuilder>() != null );

            ConnectionString = value;
            return this;
        }

        /// <summary>
        /// Applies the specified saga store table name to the configuration.
        /// </summary>
        /// <param name="tableName">The configured saga store table name. The default value is "Messaging_Saga".</param>
        /// <returns>The original <see cref="IsamSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual IsamSagaStorageConfigurationBuilder HasTableName( string tableName )
        {
            Arg.NotNullOrEmpty( tableName, nameof( tableName ) );
            Contract.Ensures( Contract.Result<IsamSagaStorageConfigurationBuilder>() != null );

            TableName = tableName;
            return this;
        }

        /// <summary>
        /// Applies the specified message type resolver to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageTypeResolver">message type resolver</see>.
        /// The default value is <see cref="DefaultMessageTypeResolver"/>.</param>
        /// <returns>The original <see cref="IsamSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual IsamSagaStorageConfigurationBuilder UseMessageTypeResolver( IMessageTypeResolver value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamSagaStorageConfigurationBuilder>() != null );

            MessageTypeResolver = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer factory to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IIsamMessageSerializerFactory">message serializer factory</see>.</param>
        /// <returns>The original <see cref="IsamSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual IsamSagaStorageConfigurationBuilder UseMessageSerializerFactory( IIsamMessageSerializerFactory value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamSagaStorageConfigurationBuilder>() != null );

            NewMessageSerializerFactory = _ => value;
            return this;
        }

        /// <summary>
        /// Applies the specified message serializer to the configuration.
        /// </summary>
        /// <param name="value">The configured factory <see cref="Func{T,TResult}">method</see> used to create a new message serializer factory.</param>
        /// <returns>The original <see cref="IsamSagaStorageConfigurationBuilder">builder</see>.</returns>
        public virtual IsamSagaStorageConfigurationBuilder UseMessageSerializerFactory( Func<IMessageTypeResolver, IIsamMessageSerializerFactory> value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<IsamSagaStorageConfigurationBuilder>() != null );

            NewMessageSerializerFactory = value;
            return this;
        }

        /// <summary>
        /// Creates and returns a new ISAM saga store configuration.
        /// </summary>
        /// <returns>A new <see cref="IsamSagaStorageConfiguration">ISAM database saga store configuration</see>.</returns>
        public virtual IsamSagaStorageConfiguration CreateConfiguration()
        {
            Contract.Ensures( Contract.Result<IsamSagaStorageConfiguration>() != null );

            return new IsamSagaStorageConfiguration(
                ConnectionString,
                TableName,
                MessageTypeResolver,
                NewMessageSerializerFactory( MessageTypeResolver ) );
        }
    }
}