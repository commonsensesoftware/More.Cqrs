// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an aggregate snapshot store backed by a SQL database.
    /// </summary>
    /// <typeparam name="TKey">The type of aggregate key.</typeparam>
    public class SqlSnapshotStore<TKey> : ISqlSnapshotStore<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSnapshotStore{TKey}"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="SqlEventStoreConfiguration">configuration</see> used by the snapshot store.</param>
        public SqlSnapshotStore( SqlEventStoreConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the event store configuration.
        /// </summary>
        /// <value>The <see cref="SqlEventStoreConfiguration">event store configuration</see>.</value>
        protected SqlEventStoreConfiguration Configuration { get; }

        /// <summary>
        /// Loads a snapshot for the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load a snapshot for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the loaded <see cref="SqlSnapshotDescriptor{TKey}">snapshot</see>
        /// or <c>null</c> if no snapshot is found.</returns>
        public virtual async Task<SqlSnapshotDescriptor<TKey>> Load( TKey aggregateId, CancellationToken cancellationToken )
        {
            using ( var connection = Configuration.CreateConnection() )
            {
                return await Load( connection, aggregateId, cancellationToken ).ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Loads a snapshot for the specified aggregate identifier.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection">connection</see> used to load snapshots.</param>
        /// <param name="aggregateId">The identifier of the aggregate to load a snapshot for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the loaded <see cref="SqlSnapshotDescriptor{TKey}">snapshot</see>
        /// or <c>null</c> if no snapshot is found.</returns>
        public virtual Task<SqlSnapshotDescriptor<TKey>> Load( DbConnection connection, TKey aggregateId, CancellationToken cancellationToken )
        {
            Arg.NotNull( connection, nameof( connection ) );
            return Configuration.LoadSnapshot( connection, aggregateId, cancellationToken );
        }

        /// <summary>
        /// Saves a snapshot.
        /// </summary>
        /// <param name="snapshots">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ISnapshot{TKey}">snapshots</see> to save.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task Save( IEnumerable<ISnapshot<TKey>> snapshots, CancellationToken cancellationToken )
        {
            Arg.NotNull( snapshots, nameof( snapshots ) );

            using ( var connection = Configuration.CreateConnection() )
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );

                using ( var command = Configuration.NewSaveSnapshotCommand() )
                {
                    command.Connection = connection;

                    using ( var transaction = connection.BeginTransaction() )
                    {
                        command.Transaction = transaction;

                        foreach ( var snapshot in snapshots )
                        {
                            using ( var snapshotDescriptor = CreateDescriptor( snapshot ) )
                            {
                                await Configuration.SaveSnapshot( command, snapshotDescriptor, cancellationToken ).ConfigureAwait( false );
                            }
                        }

                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Creates and returns a descriptor for the specified snapshot.
        /// </summary>
        /// <param name="snapshot">The <see cref="ISnapshot{TKey}">snapshot</see> to create a descriptor for.</param>
        /// <returns>A new <see cref="SqlSnapshotDescriptor{TKey}">snapshot descriptor</see>.</returns>
        protected virtual SqlSnapshotDescriptor<TKey> CreateDescriptor( ISnapshot<TKey> snapshot )
        {
            Arg.NotNull( snapshot, nameof( snapshot ) );
            Contract.Ensures( Contract.Result<SqlSnapshotDescriptor<TKey>>() != null );

            var type = snapshot.GetType().GetTypeInfo();

            if ( !( snapshot is IEvent @event ) )
            {
                throw new ArgumentException( SR.SnapshotMustAlsoBeEvent.FormatDefault( GetType().Name, type.Name ) );
            }

            return new SqlSnapshotDescriptor<TKey>()
            {
                AggregateId = snapshot.Id,
                SnapshotType = type.GetAssemblyQualifiedName(),
                Snapshot = Configuration.EventSerializer.Serialize( @event ),
                Version = snapshot.Version,
            };
        }
    }
}