// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using More.Domain.Options;
    using static Domain.Options.DefaultOptions;

    /// <summary>
    /// Represents a message descriptor for an event.
    /// </summary>
    /// <typeparam name="TKey">The key type for the event.</typeparam>
    public class EventDescriptor<TKey> : MessageDescriptor<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventDescriptor{TKey}"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier associated with the event.</param>
        /// <param name="event">The <see cref="IEvent">event</see> the descriptor is for.</param>
        public EventDescriptor( TKey aggregateId, IEvent @event ) : base( aggregateId, @event, None ) => Event = @event;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDescriptor{TKey}"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier associated with the event.</param>
        /// <param name="event">The <see cref="IEvent">event</see> the descriptor is for.</param>
        /// <param name="options">The <see cref="IOptions">options</see> associated with the <paramref name="event"/>.</param>
        public EventDescriptor( TKey aggregateId, IEvent @event, IOptions options ) : base( aggregateId, @event, options ) => Event = @event;

        /// <summary>
        /// Gets the unique message identifier.
        /// </summary>
        /// <value>The unique message identifier.</value>
        public override string MessageId => $"{AggregateId}.{Event.Version}.{Event.Sequence}";

        /// <summary>
        /// Gets the described event.
        /// </summary>
        /// <value>The described <see cref="IEvent">event</see>.</value>
        public IEvent Event { get; }
    }
}