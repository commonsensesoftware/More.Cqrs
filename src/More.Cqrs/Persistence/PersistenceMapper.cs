// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an object that maps persistence.
    /// </summary>
    public class PersistenceMapper : IMapPersistence
    {
        readonly Func<string, IPersistence?> resolve;
        readonly Dictionary<string, IPersistence> mapping = new Dictionary<string, IPersistence>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceMapper"/> class.
        /// </summary>
        public PersistenceMapper() => resolve = name => default;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceMapper"/> class.
        /// </summary>
        /// <param name="resolver">The resolver <see cref="Func{TArg,TResult}">function</see> used to resolve persistence mappings (ex: dependency injection).</param>
        public PersistenceMapper( Func<string, IPersistence> resolver ) => resolve = resolver;

        /// <summary>
        /// Adds the mapping for the specified transaction log.
        /// </summary>
        /// <param name="entityName">The name of the entity to map the persistence for.</param>
        /// <param name="persistence">The <see cref="IPersistence">persistence</see> to map.</param>
        public virtual void Add( string entityName, IPersistence persistence )
        {
            try
            {
                mapping.Add( entityName, persistence );
            }
            catch ( ArgumentException )
            {
                throw new PersistenceConfigurationException( SR.DuplicatePersistence.FormatDefault( entityName ) );
            }
        }

        /// <summary>
        /// Returns the mapped persistence.
        /// </summary>
        /// <param name="entityName">The name of the entity to map the persistence for.</param>
        /// <returns>The mapped <see cref="IPersistence">persistence</see>.</returns>
        public virtual IPersistence Map( string entityName )
        {
            var persistence = default( IPersistence );

            try
            {
                persistence = resolve( entityName );
            }
            catch ( Exception inner )
            {
                throw new PersistenceConfigurationException( SR.MissingPersistence.FormatDefault( entityName ), inner );
            }

            if ( persistence == null )
            {
                if ( !mapping.TryGetValue( entityName, out persistence ) )
                {
                    throw new PersistenceConfigurationException( SR.MissingPersistence.FormatDefault( entityName ) );
                }
            }

            return persistence;
        }
    }
}