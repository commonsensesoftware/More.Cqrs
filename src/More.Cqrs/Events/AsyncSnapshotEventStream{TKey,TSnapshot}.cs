// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an asynchronous stream of <see cref="IEvent">events</see> that contains a <see cref="ISnapshot{TKey}">snapshot</see>.
    /// </summary>
    /// <typeparam name="TKey">The type of event key.</typeparam>
    /// <typeparam name="TSnapshot">The type of snapshot.</typeparam>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class AsyncSnapshotEventStream<TKey, TSnapshot> : IAsyncEnumerable<IEvent>
        where TKey : notnull
        where TSnapshot : class, ISnapshot<TKey>, IEvent
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        readonly IAsyncEnumerable<IEvent> events;
        readonly IEvent snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSnapshotEventStream{TKey,TSnapshot}"/> class.
        /// </summary>
        /// <param name="events">The <see cref="IAsyncEnumerable{T}">sequence</see> of <see cref="IEvent">events</see> in the stream.</param>
        /// <param name="snapshot">The <typeparamref name="TSnapshot">snapshot</typeparamref> associated with the stream.</param>
        public AsyncSnapshotEventStream( IAsyncEnumerable<IEvent> events, TSnapshot snapshot )
        {
            this.events = events;
            this.snapshot = snapshot;
        }

        /// <inheritdoc />
        public async IAsyncEnumerator<IEvent> GetAsyncEnumerator( CancellationToken cancellationToken = default )
        {
            yield return snapshot;

            await foreach ( var @event in events.WithCancellation( cancellationToken ).ConfigureAwait( false ) )
            {
                if ( @event.Version > snapshot.Version )
                {
                    yield return @event;
                }
            }
        }
    }
}