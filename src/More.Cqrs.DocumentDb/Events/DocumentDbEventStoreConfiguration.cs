// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Microsoft.Azure.Documents.Client;
    using More.Domain.Messaging;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics.Contracts;
    using static Microsoft.Azure.Documents.Client.UriFactory;
    using static Newtonsoft.Json.JsonSerializer;
    using static System.StringComparer;

    /// <summary>
    /// Represents the configuration for a DocumentDb event store.
    /// </summary>
    public class DocumentDbEventStoreConfiguration
    {
        readonly IDocumentClientFactory clientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbEventStoreConfiguration"/> class.
        /// </summary>
        /// <param name="entityName">The name of the entity the event store is for.</param>
        /// <param name="clientFactory">The <see cref="IDocumentClientFactory">factory</see> used to create <see cref="DocumentClient">DocumentDb clients</see>.</param>
        /// <param name="database">The name of event store database.</param>
        /// <param name="collectionName">The name of event store collection.</param>
        /// <param name="partitionKey">The event store partition key. This parameter can be <c>null</c> if the event store is not partitioned.</param>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</param>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings">settings</see> used to serialize and deserialize messages.</param>
        [CLSCompliant( false )]
        public DocumentDbEventStoreConfiguration(
            string entityName,
            IDocumentClientFactory clientFactory,
            string database,
            string collectionName,
            object partitionKey,
            IMessageTypeResolver messageTypeResolver,
            JsonSerializerSettings serializerSettings )
        {
            Arg.NotNullOrEmpty( entityName, nameof( entityName ) );
            Arg.NotNull( clientFactory, nameof( clientFactory ) );
            Arg.NotNullOrEmpty( database, nameof( database ) );
            Arg.NotNullOrEmpty( collectionName, nameof( collectionName ) );
            Arg.NotNull( messageTypeResolver, nameof( messageTypeResolver ) );
            Arg.NotNull( serializerSettings, nameof( serializerSettings ) );

            EntityName = entityName;
            this.clientFactory = clientFactory;
            Database = database;
            CollectionName = collectionName;
            CollectionLink = CreateDocumentCollectionUri( database, collectionName );
            PartitionKey = partitionKey;
            MessageTypeResolver = messageTypeResolver;
            JsonSerializer = Create( serializerSettings );
        }

        /// <summary>
        /// Gets the name of the entity the event store is for.
        /// </summary>
        /// <value>The name of the entity the event store is for.</value>
        /// <remarks>The entity name is the logical name of the entity stored in the event store. The name might be the same as the
        /// <see cref="CollectionName">collection name</see> when Collection-Per-Entity mapping is used. The specified name is also
        /// meant to be used as a key to uniquely identify the event store.</remarks>
        public string EntityName { get; }

        /// <summary>
        /// Gets the name of the event store database.
        /// </summary>
        /// <value>The event store database name.</value>
        public string Database { get; }

        /// <summary>
        /// Gets the name of the event store document collection.
        /// </summary>
        /// <value>The event store document collection name.</value>
        public string CollectionName { get; }

        /// <summary>
        /// Gets event store collection link.
        /// </summary>
        /// <value>The <see cref="Uri">URL</see> for the event store collection.</value>
        public Uri CollectionLink { get; }

        /// <summary>
        /// Gets the event store partition key.
        /// </summary>
        /// <value>The event store partition key or <c>null</c> if the event store is not partitioned.</value>
        public object PartitionKey { get; }

        /// <summary>
        /// Gets the message type resolver used to deserialize messages.
        /// </summary>
        /// <value>The <see cref="IMessageTypeResolver">message type resolver</see> used to deserialize messages.</value>
        public IMessageTypeResolver MessageTypeResolver { get; }

        /// <summary>
        /// Gets the serializer used to serialize and deserialize messages in the JSON format.
        /// </summary>
        /// <value>The <see cref="JsonSerializer">serializer</see> used to serialize and deserialize messages.</value>
        public JsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Creates and returns new DocumentDb client.
        /// </summary>
        /// <returns>A new, configured <see cref="DocumentClient">DocumentDb client</see>.</returns>
        [CLSCompliant( false )]
        public virtual DocumentClient CreateClient()
        {
            Contract.Ensures( Contract.Result<DocumentClient>() != null );
            return clientFactory.NewClient();
        }
    }
}