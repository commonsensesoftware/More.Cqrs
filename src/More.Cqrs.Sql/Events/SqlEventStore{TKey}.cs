// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Data.CommandBehavior;

    /// <summary>
    /// Represents an event store backed by a SQL database.
    /// </summary>
    /// <typeparam name="TKey">The type of key used for events.</typeparam>
    public class SqlEventStore<TKey> : EventStore<TKey> where TKey : notnull
    {
        readonly SnapshotSerializer snapshotSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlEventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="SqlPersistence">persistence</see> associated with the event store.</param>
        public SqlEventStore( SqlPersistence persistence ) : base( persistence )
        {
            Configuration = persistence.Configuration.Events;
            Snapshots = Configuration.CreateSnapshotStore<TKey>();
            snapshotSerializer = new SnapshotSerializer( typeof( TKey ), Configuration );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlEventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        /// <param name="configuration">The <see cref="SqlEventStoreConfiguration">configuration</see> used by the event store.</param>
        public SqlEventStore( IPersistence persistence, SqlEventStoreConfiguration configuration ) : base( persistence )
        {
            Configuration = configuration;
            Snapshots = configuration.CreateSnapshotStore<TKey>();
            snapshotSerializer = new SnapshotSerializer( typeof( TKey ), configuration );
        }

        /// <summary>
        /// Gets the event store configuration.
        /// </summary>
        /// <value>The <see cref="SqlEventStoreConfiguration">event store configuration</see>.</value>
        protected SqlEventStoreConfiguration Configuration { get; }

        /// <summary>
        /// Gets the configured snapshot store.
        /// </summary>
        /// <value>The configured <see cref="ISqlSnapshotStore{TKey}">snapshot store</see>.</value>
        protected ISqlSnapshotStore<TKey> Snapshots { get; }

        /// <summary>
        /// Returns a new event stream that contains a snapshot using the specified descriptor.
        /// </summary>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> representing the <see cref="IEvent">event</see> stream.</param>
        /// <param name="snapshotDescriptor">The <see cref="SqlSnapshotDescriptor{TKey}">descriptor</see> describing the
        /// <see cref="ISnapshot{TKey}">snapshot</see> contained in the stream.</param>
        /// <returns>A new <see cref="IAsyncEnumerable{T}">asynchronous sequence</see> of <see cref="IEvent">events</see> containing a
        /// <see cref="ISnapshot{TKey}">snapshot</see>.</returns>
        protected virtual IAsyncEnumerable<IEvent> NewEventStreamWithSnapshot( IAsyncEnumerable<IEvent> events, SqlSnapshotDescriptor<TKey> snapshotDescriptor ) =>
            snapshotSerializer.Deserialize( events, snapshotDescriptor );

        /// <summary>
        /// Occurs when a sequence of events for an aggregate are loaded.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="predicate">The optional <see cref="IEventPredicate{TKey}">predicate</see> used to filter events.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        protected override IAsyncEnumerable<IEvent> OnLoad( TKey aggregateId, IEventPredicate<TKey>? predicate = default ) =>
            new Enumerable( this, aggregateId, predicate );

        private readonly struct Enumerable : IAsyncEnumerable<IEvent>
        {
            readonly SqlEventStore<TKey> eventStore;
            readonly TKey aggregateId;
            readonly IEventPredicate<TKey>? eventPredicate;

            internal Enumerable(
                SqlEventStore<TKey> eventStore,
                TKey aggregateId,
                IEventPredicate<TKey>? predicate )
            {
                this.eventStore = eventStore;
                this.aggregateId = aggregateId;
                eventPredicate = predicate;
            }

            public async IAsyncEnumerator<IEvent> GetAsyncEnumerator( CancellationToken cancellationToken = default )
            {
                var predicate = eventPredicate;
                var snapshot = default( SqlSnapshotDescriptor<TKey> );
                var configuration = eventStore.Configuration;
                using var connection = configuration.CreateConnection();

                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                // TODO: is it possible to support a predicate and snapshot simultaneously?
                if ( predicate == null )
                {
                    snapshot = await eventStore.Snapshots.Load( connection, aggregateId, cancellationToken ).ConfigureAwait( false );

                    if ( snapshot != null )
                    {
                        predicate = new EventVersionPredicate<TKey>( snapshot.Version );
                    }
                }

                using var command = configuration.NewLoadEventsCommand( aggregateId, predicate );
                command.Connection = connection;

                using var reader = await command.ExecuteReaderAsync( CloseConnection, cancellationToken ).ConfigureAwait( false );
                var events = configuration.ReadEvents( reader, cancellationToken );
                var stream = snapshot == null ? events : eventStore.NewEventStreamWithSnapshot( events, snapshot );

                await foreach ( var @event in stream.WithCancellation( cancellationToken ).ConfigureAwait( false ) )
                {
                    yield return @event;
                }
            }
        }
    }
}