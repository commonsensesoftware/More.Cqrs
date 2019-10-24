// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the behavior of an event store.
    /// </summary>
    /// <typeparam name="TKey">The key used by the stored events.</typeparam>
    public interface IEventStore<TKey> where TKey : notnull
    {
        /// <summary>
        /// Loads a sequence of events for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to load the events for.</param>
        /// <param name="predicate">The optional <see cref="IEventPredicate{TKey}">predicate</see> used to filter events.</param>
        /// <returns>A <see cref="IAsyncEnumerable{T}">asynchronous sequence</see> of <see cref="IEvent">events</see> loaded for the aggregate.</returns>
        IAsyncEnumerable<IEvent> Load( TKey aggregateId, IEventPredicate<TKey>? predicate = default );

        /// <summary>
        /// Saves a sequence of events for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The identifier of the aggregate to save the events for.</param>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> to save.</param>
        /// <param name="expectedVersion">The current, expected version of the aggregate being saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        Task Save( TKey aggregateId, IEnumerable<IEvent> events, int expectedVersion, CancellationToken cancellationToken );
    }
}