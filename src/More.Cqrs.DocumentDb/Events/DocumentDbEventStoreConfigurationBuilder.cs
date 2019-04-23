// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System.Diagnostics.Contracts;
    using static Newtonsoft.Json.NullValueHandling;
    using static Newtonsoft.Json.TypeNameHandling;

    /// <summary>
    /// Represents an object that can be used to build a <see cref="DocumentDbEventStoreConfiguration">DocumentDb event store configuration</see>.
    /// </summary>
    public class DocumentDbEventStoreConfigurationBuilder
    {
        /// <summary>
        /// Gets the logical name of the entity the stored documents correspond to.
        /// </summary>
        /// <value>The logical name of the storage entity.</value>
        protected string EntityName { get; private set; }

        /// <summary>
        /// Gets the name of the document database.
        /// </summary>
        /// <value>The name of the document database.</value>
        protected string Database { get; private set; }

        /// <summary>
        /// Gets the name of the document collection.
        /// </summary>
        /// <value>The name of the document collection.</value>
        protected string CollectionName { get; private set; }

        /// <summary>
        /// Gets the key of the partition the documents are stored in.
        /// </summary>
        /// <value>The corresponding partition key.</value>
        protected object PartitionKey { get; private set; }

        /// <summary>
        /// Gets the object used to resolve message types.
        /// </summary>
        /// <value>A <see cref="IMessageTypeResolver">message type resolver</see>.</value>
        protected IMessageTypeResolver MessageTypeResolver { get; private set; } = new DefaultMessageTypeResolver();

        /// <summary>
        /// Gets the JSON serializer settings.
        /// </summary>
        /// <value>The configured <see cref="JsonSerializerSettings">JSON serializer settings</see>.</value>
        protected JsonSerializerSettings SerializerSettings { get; private set; } = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy( processDictionaryKeys: true, overrideSpecifiedNames: true ),
            },
            NullValueHandling = Ignore,
            TypeNameHandling = Objects,
        };

        /// <summary>
        /// Applies the specified entity name to the configuration.
        /// </summary>
        /// <param name="value">The configured entity name. The default value is the same as the <see cref="CollectionName"/>.</param>
        /// <returns>The original <see cref="DocumentDbEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual DocumentDbEventStoreConfigurationBuilder HasEntityName( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<DocumentDbEventStoreConfigurationBuilder>() != null );

            EntityName = value;
            return this;
        }

        /// <summary>
        /// Gets a factory used to configure and create DocumentDb clients.
        /// </summary>
        /// <value>A <see cref="DocumentClientFactory">DocumentDb client factory</see>.</value>
        public virtual DocumentClientFactory DocumentClient { get; } = new DocumentClientFactory();

        /// <summary>
        /// Applies the specified event store database to the configuration.
        /// </summary>
        /// <param name="value">The configured event store database.</param>
        /// <returns>The original <see cref="DocumentDbEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual DocumentDbEventStoreConfigurationBuilder UseDatabase( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<DocumentDbEventStoreConfigurationBuilder>() != null );

            Database = value;
            return this;
        }

        /// <summary>
        /// Applies the specified event store collection name to the configuration.
        /// </summary>
        /// <param name="value">The configured event store collection name.</param>
        /// <returns>The original <see cref="DocumentDbEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual DocumentDbEventStoreConfigurationBuilder HasCollectionName( string value )
        {
            Arg.NotNullOrEmpty( value, nameof( value ) );
            Contract.Ensures( Contract.Result<DocumentDbEventStoreConfigurationBuilder>() != null );

            CollectionName = value;
            return this;
        }

        /// <summary>
        /// Applies the specified event store partition key to the configuration.
        /// </summary>
        /// <param name="value">The configured event store partition key. The default value is <c>null</c>.</param>
        /// <returns>The original <see cref="DocumentDbEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual DocumentDbEventStoreConfigurationBuilder HasPartitionKey( object value )
        {
            Contract.Ensures( Contract.Result<DocumentDbEventStoreConfigurationBuilder>() != null );

            PartitionKey = value;
            return this;
        }

        /// <summary>
        /// Applies the specified message type resolver to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="IMessageTypeResolver">message type resolver</see>.
        /// The default value is <see cref="DefaultMessageTypeResolver"/>.</param>
        /// <returns>The original <see cref="DocumentDbEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual DocumentDbEventStoreConfigurationBuilder UseMessageTypeResolver( IMessageTypeResolver value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<DocumentDbEventStoreConfigurationBuilder>() != null );

            MessageTypeResolver = value;
            return this;
        }

        /// <summary>
        /// Applies the specified JSON serialization settings to the configuration.
        /// </summary>
        /// <param name="value">The configured <see cref="JsonSerializerSettings"/>.</param>
        /// <returns>The original <see cref="DocumentDbEventStoreConfigurationBuilder">builder</see>.</returns>
        public virtual DocumentDbEventStoreConfigurationBuilder UseJsonSerializerSettings( JsonSerializerSettings value )
        {
            Arg.NotNull( value, nameof( value ) );
            Contract.Ensures( Contract.Result<DocumentDbEventStoreConfigurationBuilder>() != null );

            SerializerSettings = value;
            return this;
        }

        /// <summary>
        /// Creates and returns a new DocumentDb event store configuration.
        /// </summary>
        /// <returns>A new <see cref="DocumentDbEventStoreConfiguration">DocumentDb event store configuration</see>.</returns>
        public virtual DocumentDbEventStoreConfiguration CreateConfiguration()
        {
            Contract.Ensures( Contract.Result<DocumentDbEventStoreConfiguration>() != null );

            return new DocumentDbEventStoreConfiguration(
                EntityName ?? CollectionName,
                DocumentClient,
                Database,
                CollectionName,
                PartitionKey,
                MessageTypeResolver,
                SerializerSettings );
        }
    }
}