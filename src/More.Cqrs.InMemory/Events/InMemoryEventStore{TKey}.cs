// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Linq.Enumerable;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents an in-memory <see cref="IEventStore{TKey}">event store</see>.
    /// </summary>
    /// <typeparam name="TKey">The type of event key.</typeparam>
    public class InMemoryEventStore<TKey> : EventStore<TKey>
    {
        readonly ConcurrentDictionary<TKey, List<IEvent>> table = new ConcurrentDictionary<TKey, List<IEvent>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStore{TKey}"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> for the event store.</param>
        public InMemoryEventStore( IPersistence persistence ) : base( persistence ) { }

        /// <summary>
        /// Occurs when a sequence of events for an aggregate are loaded.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        protected override Task<IEnumerable<IEvent>> OnLoad( TKey aggregateId, CancellationToken cancellationToken ) =>
            table.TryGetValue( aggregateId, out var events ) ? FromResult( events.AsEnumerable() ) : FromResult( Empty<IEvent>() );

        /// <summary>
        /// Saves a sequence of events for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to save the events for.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to save.</param>
        /// <param name="expectedVersion">The expected version of the aggregate being saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public override async Task Save( TKey aggregateId, IEnumerable<IEvent> events, int expectedVersion, CancellationToken cancellationToken )
        {
            await base.Save( aggregateId, events, expectedVersion, cancellationToken ).ConfigureAwait( false );
            table.GetOrAdd( aggregateId, key => new List<IEvent>() ).AddRange( events );
        }
    }
}