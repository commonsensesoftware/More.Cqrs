// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a collection of <see cref="SagaMetadata">saga metadata</see>.
    /// </summary>
    public class SagaMetadataCollection : IReadOnlyList<SagaMetadata>
    {
        readonly KeyedCollection<Type, SagaMetadata> byData = new KeyedCollection<Type, SagaMetadata>( m => m.SagaDataType );
        readonly Dictionary<Type, SagaMetadata> bySaga = new Dictionary<Type, SagaMetadata>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaMetadataCollection"/> class.
        /// </summary>
        /// <param name="availableTypes">A <see cref="IEnumerable{T}">sequence</see> of all available types in the system.</param>
        public SagaMetadataCollection( IEnumerable<Type> availableTypes )
        {
            Arg.NotNull( availableTypes, nameof( availableTypes ) );

            var discoveredMetadata = from type in availableTypes
                                     where type.IsSaga()
                                     select SagaMetadata.Create( type );

            foreach ( var metadata in discoveredMetadata )
            {
                bySaga.Add( metadata.SagaType, metadata );
                byData.Add( metadata );
            }
        }

        /// <summary>
        /// Gets the item in the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to retrieve.</param>
        /// <returns>The <see cref="SagaMetadata"/> at the specified <paramref name="index"/>.</returns>
        public virtual SagaMetadata this[int index] => byData[index];

        /// <summary>
        /// Gets the total number of items in the collection.
        /// </summary>
        /// <value>The number of items in the collection.</value>
        public virtual int Count => byData.Count;

        /// <summary>
        /// Returns an iterator that can be used to enumerate the collection.
        /// </summary>
        /// <returns>A new <see cref="IEnumerator{T}"/> object.</returns>
        public virtual IEnumerator<SagaMetadata> GetEnumerator() => byData.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Finds the item in the collection matching the specified saga type.
        /// </summary>
        /// <param name="sagaType">The type of saga to match.</param>
        /// <returns>The matched <see cref="SagaMetadata">saga metadata</see>.</returns>
        public virtual SagaMetadata Find( Type sagaType ) => bySaga[sagaType];

        /// <summary>
        /// Finds the item in the collection matching the specified saga data type.
        /// </summary>
        /// <param name="dataType">The type of saga data to match.</param>
        /// <returns>The matched <see cref="SagaMetadata">saga metadata</see>.</returns>
        public virtual SagaMetadata FindByData( Type dataType ) => byData[dataType];
    }
}