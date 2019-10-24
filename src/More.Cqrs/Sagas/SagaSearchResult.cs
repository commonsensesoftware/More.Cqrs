// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a saga search result.
    /// </summary>
    public class SagaSearchResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SagaSearchResult"/> class.
        /// </summary>
        public SagaSearchResult()
        {
            Data = default;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaSearchResult"/> class.
        /// </summary>
        /// <param name="data">The saga data associated with the result.</param>
        public SagaSearchResult( ISagaData? data )
        {
            Data = data;

            var properties = new Dictionary<string, object>();

            if ( data != null )
            {
                properties[nameof( ISagaData.Id )] = data.Id;
            }

            Properties = properties;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaSearchResult"/> class.
        /// </summary>
        /// <param name="data">The saga data associated with the result.</param>
        /// <param name="properties">The <see cref="IReadOnlyDictionary{TKey,TValue}">read-only collection</see> of property names
        /// and values used to find the saga data.</param>
        public SagaSearchResult( ISagaData? data, IReadOnlyDictionary<string, object> properties )
        {
            Data = data;
            Properties = properties;
        }

        /// <summary>
        /// Gets the saga data for the search result.
        /// </summary>
        /// <value>The found saga data or <c>null</c>.</value>
        public ISagaData? Data { get; }

        /// <summary>
        /// Gets a read-only collection of property names and values used to find the saga data.
        /// </summary>
        /// <value>A <see cref="IReadOnlyDictionary{TKey,TValue}">read-only collection</see> of property names and values.</value>
        public IReadOnlyDictionary<string, object> Properties { get; }
    }
}