// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a stream of <see cref="IEvent">events</see> that contains a <see cref="ISnapshot{TKey}">snapshot</see>.
    /// </summary>
    /// <typeparam name="TKey">The type of event key.</typeparam>
    /// <typeparam name="TSnapshot">The type of snapshot.</typeparam>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class SnapshotEventStream<TKey, TSnapshot> : IEnumerable<IEvent> where TSnapshot : class, ISnapshot<TKey>, IEvent
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        readonly IEnumerable<IEvent> events;
        readonly IEvent snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotEventStream{TKey,TSnapshot}"/> class.
        /// </summary>
        /// <param name="events">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> in the stream.</param>
        /// <param name="snapshot">The <typeparamref name="TSnapshot">snapshot</typeparamref> associated with the stream.</param>
        public SnapshotEventStream( IEnumerable<IEvent> events, TSnapshot snapshot )
        {
            Arg.NotNull( events, nameof( events ) );
            Arg.NotNull( snapshot, nameof( snapshot ) );

            this.events = events;
            this.snapshot = snapshot;
        }

        /// <summary>
        /// Creates and returns an iterator that can be used to enumerate the stream.
        /// </summary>
        /// <returns>A new <see cref="IEnumerator{T}">enumerator</see> object.</returns>
        public virtual IEnumerator<IEvent> GetEnumerator()
        {
            yield return snapshot;

            foreach ( var @event in events.SkipWhile( e => e.Version <= snapshot.Version ).Reverse() )
            {
                yield return @event;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}