// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using More.Domain.Events;
    using System;

    /// <summary>
    /// Represents the base implementation for a repository of aggregates whose keys are a globally unique identifier (GUID).
    /// </summary>
    /// <typeparam name="T">The type of aggregates in the repository.</typeparam>
    public class Repository<T> : Repository<Guid, T> where T : class, IAggregate<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="eventStore">The underlying <see cref="IEventStore{TKey}">event store</see> used by the repository.</param>
        public Repository( IEventStore<Guid> eventStore ) : base( eventStore ) { }
    }
}