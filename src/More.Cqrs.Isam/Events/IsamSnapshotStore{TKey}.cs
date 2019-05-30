// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Microsoft.Database.Isam;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Represents an aggregate snapshot store backed by an ISAM database.
    /// </summary>
    /// <typeparam name="TKey">The type of aggregate key.</typeparam>
    public class IsamSnapshotStore<TKey> : IIsamSnapshotStore<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsamSnapshotStore{TKey}"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IsamEventStoreConfiguration">configuration</see> used by the snapshot store.</param>
        public IsamSnapshotStore( IsamEventStoreConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the event store configuration.
        /// </summary>
        /// <value>The <see cref="IsamEventStoreConfiguration">event store configuration</see>.</value>
        protected IsamEventStoreConfiguration Configuration { get; }

        /// <summary>
        /// Loads a snapshot for the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load a snapshot for.</param>
        /// <returns>The loaded <see cref="IsamSnapshotDescriptor{TKey}">snapshot</see> or <c>null</c> if no snapshot is found.</returns>
        public virtual IsamSnapshotDescriptor<TKey> Load( TKey aggregateId )
        {
            var connection = Configuration.CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                {
                    return Load( database, aggregateId );
                }
            }
        }

        /// <inheritdoc />
        public virtual IsamSnapshotDescriptor<TKey> Load( IsamDatabase database, TKey aggregateId ) =>
            Configuration.LoadSnapshot( database, aggregateId );

        /// <summary>
        /// Saves a snapshot.
        /// </summary>
        /// <param name="snapshots">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ISnapshot{TKey}">snapshots</see> to save.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        public virtual void Save( IEnumerable<ISnapshot<TKey>> snapshots, CancellationToken cancellationToken )
        {
            Arg.NotNull( snapshots, nameof( snapshots ) );

            var connection = Configuration.CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                using ( var cursor = database.OpenCursor( Configuration.Snapshots.TableName ) )
                using ( var transaction = new IsamTransaction( session ) )
                {
                    foreach ( var snapshot in snapshots )
                    {
                        using ( var snapshotDescriptor = CreateDescriptor( snapshot ) )
                        {
                            Configuration.SaveSnapshot( cursor, snapshotDescriptor );
                        }

                        if ( cancellationToken.IsCancellationRequested )
                        {
                            transaction.Rollback();
                            break;
                        }
                    }

                    if ( !cancellationToken.IsCancellationRequested )
                    {
                        transaction.Commit();
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Creates and returns a descriptor for the specified snapshot.
        /// </summary>
        /// <param name="snapshot">The <see cref="ISnapshot{TKey}">snapshot</see> to create a descriptor for.</param>
        /// <returns>A new <see cref="IsamSnapshotDescriptor{TKey}">snapshot descriptor</see>.</returns>
        protected virtual IsamSnapshotDescriptor<TKey> CreateDescriptor( ISnapshot<TKey> snapshot )
        {
            Arg.NotNull( snapshot, nameof( snapshot ) );
            Contract.Ensures( Contract.Result<IsamSnapshotDescriptor<TKey>>() != null );

            var type = snapshot.GetType().GetTypeInfo();

            if ( !( snapshot is IEvent @event ) )
            {
                throw new ArgumentException( SR.SnapshotMustAlsoBeEvent.FormatDefault( GetType().Name, type.Name ) );
            }

            return new IsamSnapshotDescriptor<TKey>()
            {
                AggregateId = snapshot.Id,
                SnapshotType = type.GetAssemblyQualifiedName(),
                Snapshot = Configuration.EventSerializer.Serialize( @event ),
                Version = snapshot.Version,
            };
        }
    }
}