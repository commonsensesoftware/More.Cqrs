// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Messaging;
    using Options;
    using System.Diagnostics;
    using static Options.DefaultOptions;

    /// <summary>
    /// Represents a <see cref="ISnapshot{TKey}">snapshot</see> represented as an <see cref="IEvent">event</see>.
    /// </summary>
    /// <typeparam name="TKey">The type of key for the snapshot.</typeparam>
    [DebuggerDisplay( "{GetType().Name}, Version = {Version}, AggregateId = {AggregateId}" )]
    public abstract class SnapshotEvent<TKey> : IEvent, ISnapshot<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotEvent{TKey}"/> class.
        /// </summary>
        protected SnapshotEvent() { }

        /// <summary>
        /// Gets or sets the associated aggregate identifier.
        /// </summary>
        /// <value>The associated aggregate identifier.</value>
        public TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the associated aggregate version.
        /// </summary>
        /// <value>The version of the aggregate that generated the event.</value>
        public int Version { get; set; }

#pragma warning disable CA1033 // Interface methods should be callable by child types
        int IEvent.Sequence
        {
            get => 0;
            set { }
        }

        int IMessage.Revision => 1;

        string IMessage.CorrelationId => null;
#pragma warning restore CA1033 // Interface methods should be callable by child types

        /// <summary>
        /// Creates and returns a descriptor for the current event.
        /// </summary>
        /// <param name="options">The <see cref="IOptions">options</see> associated with the event. The default
        /// implementation always ignores this parameter.</param>
        /// <returns>A new <see cref="IMessageDescriptor">message descriptor</see>.</returns>
        public virtual IMessageDescriptor GetDescriptor( IOptions options )
        {
            Arg.NotNull( options, nameof( options ) );
            return new EventDescriptor<TKey>( Id, this, options: None );
        }
    }
}