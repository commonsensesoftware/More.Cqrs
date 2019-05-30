// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System;

    /// <summary>
    /// Represents an event store backed by an ISAM database with events having globally unique identifiers (GUID) for keys.
    /// </summary>
    public class IsamEventStore : IsamEventStore<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsamEventStore"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IsamPersistence">persistence</see> associated with the event store.</param>
        public IsamEventStore( IsamPersistence persistence ) : base( persistence ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamEventStore"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        /// <param name="configuration">The <see cref="IsamEventStoreConfiguration">configuration</see> used by the event store.</param>
        public IsamEventStore( IPersistence persistence, IsamEventStoreConfiguration configuration ) : base( persistence, configuration ) { }
    }
}