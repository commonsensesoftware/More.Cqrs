// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System;

    /// <summary>
    /// Represents the base implementation of an event store whose keys are globally unique identifiers (GUID).
    /// </summary>
    public abstract class EventStore : EventStore<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventStore"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        protected EventStore( IPersistence persistence ) : base( persistence ) { }
    }
}