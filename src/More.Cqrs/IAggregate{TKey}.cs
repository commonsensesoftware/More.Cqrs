// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Events;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of an aggregate.
    /// </summary>
    /// <typeparam name="TKey">The type of key used to identify the aggregate.</typeparam>
    [ContractClass( typeof( IAggregateContract<> ) )]
    public interface IAggregate<out TKey> : IChangeTracking
    {
        /// <summary>
        /// Gets the unique aggregate identifier.
        /// </summary>
        /// <value>The identifier of the aggregate.</value>
        TKey Id { get; }

        /// <summary>
        /// Gets the version of the aggregate.
        /// </summary>
        /// <value>The version of the aggregate.</value>
        /// <remarks>The version can be used for concurrency checks.</remarks>
        int Version { get; }

        /// <summary>
        /// Gets a read-only list of events recorded by the aggregate that have yet to be committed to storage.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of uncommitted <see cref="IEvent">events</see>.</value>
        IReadOnlyList<IEvent> UncommittedEvents { get; }

        /// <summary>
        /// Relays a historical sequence of events.
        /// </summary>
        /// <param name="history">The <see cref="IEnumerable{T}">sequence</see> of historical <see cref="IEvent">events</see> to replay.</param>
        void ReplayAll( IEnumerable<IEvent> history );

        /// <summary>
        /// Creates and returns a new snapshot of the aggregate.
        /// </summary>
        /// <returns>An opaque <see cref="ISnapshot{TKey}">snapshot</see> object for the aggregate based on its current state.</returns>
        ISnapshot<TKey> CreateSnapshot();
    }
}