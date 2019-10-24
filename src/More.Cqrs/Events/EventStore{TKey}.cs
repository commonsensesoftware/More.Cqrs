// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the base implementation of an event store.
    /// </summary>
    /// <typeparam name="TKey">The key used by the stored events.</typeparam>
    public abstract class EventStore<TKey> : IEventStore<TKey> where TKey : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        protected EventStore( IPersistence persistence ) => Persistence = persistence;

        /// <summary>
        /// Gets the associated persistence.
        /// </summary>
        /// <value>The <see cref="IPersistence">persistence</see> associated with the event store.</value>
        protected IPersistence Persistence { get; }

        /// <summary>
        /// Loads a sequence of events for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="predicate">The optional <see cref="IEventPredicate{TKey}">predicate</see> used to filter events.</param>
        /// <returns>A <see cref="IAsyncEnumerable{T}">asynchronous sequence</see> of <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        public virtual IAsyncEnumerable<IEvent> Load( TKey aggregateId, IEventPredicate<TKey>? predicate = default ) =>
            new EventStream( aggregateId, OnLoad( aggregateId, predicate ) );

        /// <summary>
        /// Occurs when a sequence of events for an aggregate are loaded.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="predicate">The optional <see cref="IEventPredicate{TKey}">predicate</see> used to filter events.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        protected abstract IAsyncEnumerable<IEvent> OnLoad( TKey aggregateId, IEventPredicate<TKey>? predicate = default );

        /// <summary>
        /// Saves a sequence of events for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to save the events for.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to save.</param>
        /// <param name="expectedVersion">The expected version of the aggregate being saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public virtual async Task Save( TKey aggregateId, IEnumerable<IEvent> events, int expectedVersion, CancellationToken cancellationToken )
        {
            var commit = new Commit() { Id = aggregateId, Version = expectedVersion + 1 };

            foreach ( var @event in events )
            {
                commit.Messages.Add( @event.GetDescriptor() );
                commit.Events.Add( @event );
            }

            if ( commit.Events.Count > 0 )
            {
                await Persistence.Persist( commit, cancellationToken ).ConfigureAwait( false );
            }
        }

        sealed class EventStream : IAsyncEnumerable<IEvent>
        {
            readonly TKey key;
            readonly IAsyncEnumerable<IEvent> stream;

            internal EventStream( TKey key, IAsyncEnumerable<IEvent> stream )
            {
                this.key = key;
                this.stream = stream;
            }

            public async IAsyncEnumerator<IEvent> GetAsyncEnumerator( CancellationToken cancellationToken = default )
            {
                await using var iterator = stream.GetAsyncEnumerator( cancellationToken );

                if ( !await iterator.MoveNextAsync().ConfigureAwait( false ) )
                {
                    throw new AggregateNotFoundException( SR.AggregateNotFound.FormatDefault( key ) );
                }

                do
                {
                    yield return iterator.Current;
                }
                while ( await iterator.MoveNextAsync().ConfigureAwait( false ) );
            }
        }
    }
}