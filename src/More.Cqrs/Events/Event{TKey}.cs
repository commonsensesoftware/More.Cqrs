// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using More.Domain.Options;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents the base implementation for an event.
    /// </summary>
    /// <typeparam name="TKey">The type of key for the event.</typeparam>
    [DebuggerDisplay( "{GetType().Name}, Version = {Version}, AggregateId = {AggregateId}" )]
    public abstract class Event<TKey> : IEvent where TKey : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Event{TKey}"/> class.
        /// </summary>
        protected Event() => CorrelationId = Correlation.CurrentId;

        /// <summary>
        /// Gets or sets the associated aggregate identifier.
        /// </summary>
        /// <value>The associated aggregate identifier.</value>
        public TKey AggregateId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the associated aggregate version.
        /// </summary>
        /// <value>The version of the aggregate that generated the event.</value>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the sequence of the event relative to the aggregate version.
        /// </summary>
        /// <value>The sequence of the generated the event. The default value is zero.</value>
        /// <remarks>A single aggregate version may generate multiple event.</remarks>
        public int Sequence { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the event.
        /// </summary>
        /// <value>The <see cref="DateTimeOffset">date and time</see> of the event.</value>
        public DateTimeOffset RecordedOn { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets the revision of the event.
        /// </summary>
        /// <value>The event revision.</value>
        /// <remarks>The event revision can be used to handle different versions of an event over time.</remarks>
        public int Revision { get; set; } = 1;

        /// <summary>
        /// Gets or sets the associated correlation identifier.
        /// </summary>
        /// <value>The associated correlation identifier.</value>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Creates and returns a descriptor for the current event.
        /// </summary>
        /// <param name="options">The <see cref="IOptions">options</see> associated with the event.</param>
        /// <returns>A new <see cref="IMessageDescriptor">message descriptor</see>.</returns>
        public virtual IMessageDescriptor GetDescriptor( IOptions options ) => new EventDescriptor<TKey>( AggregateId, this, options );
    }
}