// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents in-memory persistence.
    /// </summary>
    public class InMemoryPersistence : IPersistence
    {
        readonly CommitCollection commits = new CommitCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryPersistence"/> class.
        /// </summary>
        /// <param name="messageSender">The <see cref="IMessageSender">messgae sender</see> used to dispatch persisted messages.</param>
        /// <param name="sagaStorage">The <see cref="IStoreSagaData">object</see> used to store saga data.</param>
        public InMemoryPersistence( IMessageSender messageSender, IStoreSagaData sagaStorage )
        {
            MessageSender = messageSender;
            SagaStorage = sagaStorage;
        }

        /// <summary>
        /// Gets the message sender used to dispatch persisted messages.
        /// </summary>
        /// <value>The <see cref="IMessageSender">messgae sender</see> used to dispatch persisted messages.</value>
        protected IMessageSender MessageSender { get; }

        /// <summary>
        /// Gets the storage for sagas.
        /// </summary>
        /// <value>The <see cref="IStoreSagaData">object</see> used to store saga data.</value>
        protected IStoreSagaData SagaStorage { get; }

        /// <summary>
        /// Gets a read-only list of persisted commits.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of persisted <see cref="Commit">commits</see>.</value>
        public IReadOnlyList<Commit> Commits => commits;

        /// <summary>
        /// Resets persistence and clears all stored data from memory.
        /// </summary>
        public virtual void Reset() => commits.Reset();

        /// <summary>
        /// Persists the specified commit.
        /// </summary>
        /// <param name="commit">The <see cref="Commit">commit</see> to append.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task Persist( Commit commit, CancellationToken cancellationToken )
        {
            Persist( commit );
            await AppendEvents( commit.Id, commit.Events, commit.Version, cancellationToken ).ConfigureAwait( false );

            if ( commit.Saga != null )
            {
                await TransitionState( commit.Saga, cancellationToken ).ConfigureAwait( false );
            }

            await EnqueueMessages( commit.Messages, cancellationToken ).ConfigureAwait( false );
        }

        /// <summary>
        /// Persists the specified commit in memory.
        /// </summary>
        /// <param name="commit">The <see cref="Commit">commit</see> to persist.</param>
        /// <remarks>This method will throw <see cref="ConcurrencyException"/> if the <paramref name="commit"/> cannot be persisted.</remarks>
        protected void Persist( Commit commit ) => commits.Add( commit );

        /// <summary>
        /// Appends a set of events for the specified version using the provided command.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to append the events to.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to commit.</param>
        /// <param name="version">The version of the aggregate the events are being saved for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected virtual Task AppendEvents( object aggregateId, IEnumerable<IEvent> events, int version, CancellationToken cancellationToken )
        {
            var sequence = 0;

            foreach ( var @event in events )
            {
                @event.Version = version;
                @event.Sequence = sequence++;
            }

            return CompletedTask;
        }

        /// <summary>
        /// Transitions the state of a saga.
        /// </summary>
        /// <param name="saga">The <see cref="ISagaInstance">saga instance</see> to perform a state transition on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        /// <remarks>If the <paramref name="saga"/> is <c>null</c>, no action is performed.</remarks>
        protected virtual async Task TransitionState( ISagaInstance saga, CancellationToken cancellationToken )
        {
            if ( saga.Completed )
            {
                if ( !saga.IsNew )
                {
                    await SagaStorage.Complete( saga.Data, cancellationToken ).ConfigureAwait( false );
                }

                saga.Complete();
            }
            else
            {
                await SagaStorage.Store( saga.Data, saga.CorrelationProperty, cancellationToken ).ConfigureAwait( false );
                saga.Update();
            }
        }

        /// <summary>
        /// Enqueues the specified messages using the provided command.
        /// </summary>
        /// <param name="messageDescriptors">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IMessageDescriptor">messages</see> to enqueue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        protected virtual Task EnqueueMessages( IEnumerable<IMessageDescriptor> messageDescriptors, CancellationToken cancellationToken ) =>
            MessageSender.Send( messageDescriptors, cancellationToken );

        struct CommitKey : IEquatable<CommitKey>
        {
            internal readonly object Id;
            internal readonly int Version;

            internal CommitKey( object id, int version )
            {
                Id = id;
                Version = version;
            }

            public override bool Equals( object? obj ) => obj is CommitKey other && Equals( other );

            public bool Equals( CommitKey other ) => Id.Equals( other.Id ) && Version == other.Version;

            public override int GetHashCode() => HashCode.Combine( Id, Version );

            public override string ToString() => $"Id = {Id}, Version = {Version}";

            public static bool operator ==( CommitKey left, CommitKey right ) => left.Equals( right );

            public static bool operator !=( CommitKey left, CommitKey right ) => !left.Equals( right );
        }

        sealed class CommitCollection : KeyedCollection<CommitKey, Commit>
        {
            internal CommitCollection() : base( EqualityComparer<CommitKey>.Default, 0 ) { }

            protected override CommitKey GetKeyForItem( Commit item ) => new CommitKey( item.Id, item.Version );

            protected override void InsertItem( int index, Commit item )
            {
                try
                {
                    base.InsertItem( index, item );
                }
                catch ( ArgumentException )
                {
                    throw new ConcurrencyException( SR.AggregateVersionOutDated.FormatDefault( item.Id, item.Version ) );
                }
            }

            protected override void ClearItems() => throw new InvalidOperationException( SR.CollectionIsAppendOnly );

            protected override void RemoveItem( int index ) => throw new InvalidOperationException( SR.CollectionIsAppendOnly );

            protected override void SetItem( int index, Commit item ) => throw new InvalidOperationException( SR.CollectionIsAppendOnly );

            internal void Reset() => base.ClearItems();
        }
    }
}