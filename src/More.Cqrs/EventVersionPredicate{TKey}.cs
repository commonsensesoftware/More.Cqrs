// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    /// <summary>
    /// Represents a predicate which filters events by their version.
    /// </summary>
    /// <typeparam name="TKey">The type of event key.</typeparam>
    public class EventVersionPredicate<TKey> : IEventPredicate<TKey> where TKey : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventVersionPredicate{TKey}"/> class.
        /// </summary>
        /// <param name="version">The initial event version.</param>
        public EventVersionPredicate( int version ) => Version = version;

        /// <summary>
        /// Gets the version that events must be greater than or equal to.
        /// </summary>
        /// <value>The initial event version.</value>
        public int Version { get; }
    }
}