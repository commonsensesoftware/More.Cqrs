// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    /// <summary>
    /// Defines the behavior an aggregate snapshot.
    /// </summary>
    /// <typeparam name="TKey">The type of key used by the aggregate snapshot.</typeparam>
    public interface ISnapshot<out TKey> where TKey : notnull
    {
        /// <summary>
        /// Gets the identifier of the aggregate that the snapshot corresponds to.
        /// </summary>
        /// <value>The associated unique aggregate identifier.</value>
        TKey Id { get; }

        /// <summary>
        /// Gets the version of the snapshot.
        /// </summary>
        /// <value>The snapshot version.</value>
        /// <remarks>The snapshot version is typically the version of the associated aggregate
        /// when the snapshot was created.</remarks>
        int Version { get; }
    }
}