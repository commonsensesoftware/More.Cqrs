// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the base implementation of an event store.
    /// </summary>
    /// <typeparam name="TKey">The key used by the stored events.</typeparam>
    public abstract class EventStore<TKey> : IEventStore<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        protected EventStore( IPersistence persistence )
        {
            Arg.NotNull( persistence, nameof( persistence ) );
            Persistence = persistence;
        }

        /// <summary>
        /// Gets the associated persistence.
        /// </summary>
        /// <value>The <see cref="IPersistence">persistence</see> associated with the event store.</value>
        protected IPersistence Persistence { get; }

        /// <summary>
        /// Loads a sequence of events for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        public virtual async Task<IEnumerable<IEvent>> Load( TKey aggregateId, CancellationToken cancellationToken ) =>
            new EventStream<TKey>( aggregateId, await OnLoad( aggregateId, cancellationToken ).ConfigureAwait( false ) );

        /// <summary>
        /// Occurs when a sequence of events for an aggregate are loaded.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        protected abstract Task<IEnumerable<IEvent>> OnLoad( TKey aggregateId, CancellationToken cancellationToken );

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
            Arg.NotNull( events, nameof( events ) );

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

        sealed class EventStream<T> : IEnumerable<IEvent>
        {
            readonly T key;
            readonly IEnumerable<IEvent> stream;

            internal EventStream( T key, IEnumerable<IEvent> stream )
            {
                Contract.Requires( stream != null );

                this.key = key;
                this.stream = stream;
            }

            public IEnumerator<IEvent> GetEnumerator()
            {
                using ( var iterator = stream.GetEnumerator() )
                {
                    if ( !iterator.MoveNext() )
                    {
                        throw new AggregateNotFoundException( SR.AggregateNotFound.FormatDefault( key ) );
                    }

                    do
                    {
                        yield return iterator.Current;
                    }
                    while ( iterator.MoveNext() );
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}