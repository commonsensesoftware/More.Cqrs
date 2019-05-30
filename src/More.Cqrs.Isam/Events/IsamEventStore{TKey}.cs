// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an event store backed by an ISAM database.
    /// </summary>
    /// <typeparam name="TKey">The type of key used for events.</typeparam>
    public class IsamEventStore<TKey> : EventStore<TKey>
    {
        readonly SnapshotSerializer snapshotSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamEventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IsamPersistence">persistence</see> associated with the event store.</param>
        public IsamEventStore( IsamPersistence persistence ) : base( persistence )
        {
            Configuration = persistence.Configuration.Events;
            Snapshots = Configuration.CreateSnapshotStore<TKey>();
            snapshotSerializer = new SnapshotSerializer( typeof( TKey ), Configuration );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamEventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        /// <param name="configuration">The <see cref="IsamEventStoreConfiguration">configuration</see> used by the event store.</param>
        public IsamEventStore( IPersistence persistence, IsamEventStoreConfiguration configuration ) : base( persistence )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNullOrEmpty( configuration.EntityName, nameof( configuration.EntityName ) );

            Configuration = configuration;
            Snapshots = configuration.CreateSnapshotStore<TKey>();
            snapshotSerializer = new SnapshotSerializer( typeof( TKey ), configuration );
        }

        /// <summary>
        /// Gets the event store configuration.
        /// </summary>
        /// <value>The <see cref="IsamEventStoreConfiguration">event store configuration</see>.</value>
        protected IsamEventStoreConfiguration Configuration { get; }

        /// <summary>
        /// Gets the configured snapshot store.
        /// </summary>
        /// <value>The configured <see cref="IIsamSnapshotStore{TKey}">snapshot store</see>.</value>
        protected IIsamSnapshotStore<TKey> Snapshots { get; }

        /// <summary>
        /// Returns a new event stream that contains a snapshot using the specified descriptor.
        /// </summary>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> representing the <see cref="IEvent">event</see> stream.</param>
        /// <param name="snapshotDescriptor">The <see cref="IsamSnapshotDescriptor{TKey}">descriptor</see> describing the
        /// <see cref="ISnapshot{TKey}">snapshot</see> contained in the stream.</param>
        /// <returns>A new <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> containing a
        /// <see cref="ISnapshot{TKey}">snapshot</see>.</returns>
        protected virtual IEnumerable<IEvent> NewEventStreamWithSnapshot( IEnumerable<IEvent> events, IsamSnapshotDescriptor<TKey> snapshotDescriptor )
        {
            Arg.NotNull( events, nameof( events ) );
            Arg.NotNull( snapshotDescriptor, nameof( snapshotDescriptor ) );
            Contract.Ensures( Contract.Result<IEnumerable<IEvent>>() != null );

            return snapshotSerializer.Deserialize( events, snapshotDescriptor );
        }

        /// <summary>
        /// Occurs when a sequence of events for an aggregate are loaded.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        protected override Task<IEnumerable<IEvent>> OnLoad( TKey aggregateId, CancellationToken cancellationToken )
        {
            const int FirstVersion = 0;
            var connection = Configuration.CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var snapshot = Snapshots.Load( database, aggregateId ) )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var version = snapshot?.Version ?? FirstVersion;

                    using ( var cursor = Configuration.NewLoadEventsCursor( database, aggregateId, version ) )
                    {
                        var events = Configuration.ReadEvents( cursor, cancellationToken );
                        var result = snapshot == null ? events : NewEventStreamWithSnapshot( events, snapshot );

                        return Task.FromResult( result );
                    }
                }
            }
        }
    }
}