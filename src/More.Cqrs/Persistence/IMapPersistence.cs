// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of an object that maps persistence.
    /// </summary>
    [ContractClass( typeof( IMapPersistenceContract ) )]
    public interface IMapPersistence
    {
        /// <summary>
        /// Adds the mapping for the specified transaction log.
        /// </summary>
        /// <param name="entityName">The name of the entity to map the persistence for.</param>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> to map.</param>
        void Add( string entityName, IPersistence persistence );

        /// <summary>
        /// Returns the mapped persistence.
        /// </summary>
        /// <param name="entityName">The name of the entity to map the persistence for.</param>
        /// <returns>The mapped <see cref="IPersistence">persistence</see>.</returns>
        IPersistence Map( string entityName );
    }
}