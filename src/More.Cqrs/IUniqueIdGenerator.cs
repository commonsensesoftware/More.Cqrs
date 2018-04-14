// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    /// <summary>
    /// Defines the behavior of a unique identifier generator.
    /// </summary>
    public interface IUniqueIdGenerator
    {
        /// <summary>
        /// Generates a new, unique identifier.
        /// </summary>
        /// <returns>The new <see cref="Guid">globaly unique identifier</see>.</returns>
        Guid NewId();
    }
}