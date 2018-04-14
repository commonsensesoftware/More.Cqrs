// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System;

    /// <summary>
    /// Represents an event store backed by DocumentDb with events having globally unique identifiers (GUID) for keys.
    /// </summary>
    public class DocumentDbEventStore : DocumentDbEventStore<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbEventStore"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        /// <param name="configuration">The <see cref="DocumentDbEventStoreConfiguration">configuration</see> used by the event store.</param>
        public DocumentDbEventStore( IPersistence persistence, DocumentDbEventStoreConfiguration configuration ) : base( persistence, configuration ) { }
    }
}