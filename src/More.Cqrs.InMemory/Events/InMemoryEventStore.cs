// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System;

    /// <summary>
    /// Represents an in-memory <see cref="IEventStore{TKey}">event store</see> whose keys are globally unique identifiers (GUID).
    /// </summary>
    public class InMemoryEventStore : InMemoryEventStore<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStore"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> for the event store.</param>
        public InMemoryEventStore( IPersistence persistence ) : base( persistence ) { }
    }
}