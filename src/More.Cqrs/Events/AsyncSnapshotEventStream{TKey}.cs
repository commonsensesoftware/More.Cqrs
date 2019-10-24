// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a stream of <see cref="IEvent">events</see> that contains a <see cref="SnapshotEvent{TKey}">snapshot</see>.
    /// </summary>
    /// <typeparam name="TKey">The type of event key.</typeparam>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class AsyncSnapshotEventStream<TKey> : AsyncSnapshotEventStream<TKey, SnapshotEvent<TKey>> where TKey : notnull
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSnapshotEventStream{T}"/> class.
        /// </summary>
        /// <param name="events">The <see cref="IAsyncEnumerable{T}">asynchronous sequence</see> of <see cref="IEvent">events</see> in the stream.</param>
        /// <param name="snapshot">The <see cref="SnapshotEvent{TKey}">snapshot</see> associated with the stream.</param>
        public AsyncSnapshotEventStream( IAsyncEnumerable<IEvent> events, SnapshotEvent<TKey> snapshot ) : base( events, snapshot ) { }
    }
}