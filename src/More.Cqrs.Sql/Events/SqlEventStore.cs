// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System;

    /// <summary>
    /// Represents an event store backed by a SQL database with events having globally unique identifiers (GUID) for keys.
    /// </summary>
    public class SqlEventStore : SqlEventStore<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlEventStore"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="SqlPersistence">persistence</see> associated with the event store.</param>
        public SqlEventStore( SqlPersistence persistence ) : base( persistence ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlEventStore"/> class.
        /// </summary>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> associated with the event store.</param>
        /// <param name="configuration">The <see cref="SqlEventStoreConfiguration">configuration</see> used by the event store.</param>
        public SqlEventStore( IPersistence persistence, SqlEventStoreConfiguration configuration ) : base( persistence, configuration ) { }
    }
}