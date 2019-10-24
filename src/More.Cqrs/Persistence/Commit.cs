// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Sagas;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    /// <summary>
    /// Represents a persistence commit.
    /// </summary>
    [DebuggerDisplay( "Id = {Id}, Version = {Version}" )]
    public class Commit
    {
        /// <summary>
        /// Gets or sets the commit identifier.
        /// </summary>
        /// <value>The commit identifier.</value>
        /// <remarks>The specified identifier is typically the corresponding aggregate identifier.</remarks>
        public object Id { get; set; } = default!;

        /// <summary>
        /// Gets or sets the associated aggregate version.
        /// </summary>
        /// <value>The version of the aggregate that generated the event.</value>
        public int Version { get; set; }

        /// <summary>
        /// Gets a collection of messages to be enqueued and dispatched.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of <see cref="IMessageDescriptor">messages</see> to enqueue and dispatch.</value>
        public virtual ICollection<IMessageDescriptor> Messages { get; } = new Collection<IMessageDescriptor>();

        /// <summary>
        /// Gets the stream of events to save.
        /// </summary>
        /// <value>The <see cref="ICollection{T}">collection</see> of <see cref="IEvent">events</see> to save.</value>
        public virtual ICollection<IEvent> Events { get; } = new Collection<IEvent>();

        /// <summary>
        /// Gets or sets the associated saga.
        /// </summary>
        /// <value>The current <see cref="ISagaInstance">saga instance</see>, if any.</value>
        public ISagaInstance? Saga { get; set; }
    }
}