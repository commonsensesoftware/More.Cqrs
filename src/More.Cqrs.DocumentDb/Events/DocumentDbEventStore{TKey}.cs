// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an event store backed by DocumentDb.
    /// </summary>
    /// <typeparam name="TKey">The type of key used for events.</typeparam>
    public class DocumentDbEventStore<TKey> : EventStore<TKey>
    {
        readonly DocumentClient client;
        readonly JsonMessageSerializer<IEvent> eventSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbEventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        /// <param name="configuration">The <see cref="DocumentDbEventStoreConfiguration">configuration</see> used by the event store.</param>
        public DocumentDbEventStore( IPersistence persistence, DocumentDbEventStoreConfiguration configuration ) : base( persistence )
        {
            Arg.NotNull( configuration, nameof( configuration ) );

            Configuration = configuration;
            client = configuration.CreateClient();
            eventSerializer = new JsonMessageSerializer<IEvent>( configuration.MessageTypeResolver, configuration.JsonSerializer );
        }

        /// <summary>
        /// Gets the event store configuration.
        /// </summary>
        /// <value>The <see cref="DocumentDbEventStoreConfiguration">event store configuration</see>.</value>
        protected DocumentDbEventStoreConfiguration Configuration { get; }

        /// <summary>
        /// Deserializes the specified message.
        /// </summary>
        /// <param name="messageType">The type of message.</param>
        /// <param name="revision">The revision of the message.</param>
        /// <param name="message">The message to deserialize.</param>
        /// <returns>A deserialized <see cref="IEvent">event</see>.</returns>
        protected virtual IEvent Deserialize( string messageType, int revision, JObject message )
        {
            Arg.NotNullOrEmpty( messageType, nameof( messageType ) );
            Arg.NotNull( message, nameof( message ) );
            Contract.Ensures( Contract.Result<IEvent>() != null );

            return eventSerializer.Deserialize( messageType, revision, message );
        }

        /// <summary>
        /// Creates and returns a new document query.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier to create the query for.</param>
        /// <param name="feedOptions">The <see cref="FeedOptions">feed options</see> for the query.</param>
        /// <returns>A new, configured <see cref="IQueryable{T}">document query</see>.</returns>
        /// <remarks>If the event store is configured using a partition key, the created query will also be partitioned.</remarks>
        [CLSCompliant(false)]
        protected virtual IQueryable<RecordedEvent<TKey>> NewDocumentQuery( TKey aggregateId, FeedOptions feedOptions )
        {
            Arg.NotNull( feedOptions, nameof( feedOptions ) );
            Contract.Ensures( Contract.Result<IQueryable<RecordedEvent<TKey>>>() != null );

            var collectionLink = Configuration.CollectionLink;
            var keyValue = Configuration.PartitionKey;

            if ( keyValue == null )
            {
                feedOptions.PartitionKey = new PartitionKey( keyValue );
            }

            return client.CreateDocumentQuery<RecordedEvent<TKey>>( collectionLink, feedOptions ).Where( e => e.Id.Equals( aggregateId ) );
        }

        /// <summary>
        /// Loads a sequence of events for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        protected override async Task<IEnumerable<IEvent>> OnLoad( TKey aggregateId, CancellationToken cancellationToken )
        {
            var feedOptions = new FeedOptions();
            var query = NewDocumentQuery( aggregateId, feedOptions ).AsDocumentQuery();
            var loadedEvents = new List<RecordedEvent<TKey>>();

            IEnumerable<IEvent> Iterator( IEnumerable<RecordedEvent<TKey>> recordedEvents )
            {
                foreach ( var recordedEvent in recordedEvents )
                {
                    yield return Deserialize( recordedEvent.Type, recordedEvent.Revision, recordedEvent.Message );
                }
            }

            do
            {
                loadedEvents.AddRange( await query.ExecuteNextAsync<RecordedEvent<TKey>>( cancellationToken ).ConfigureAwait( false ) );
            }
            while ( query.HasMoreResults );

            return Iterator( loadedEvents );
        }
    }
}