// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    /// <summary>
    /// Represents the default unique identifier generator.
    /// </summary>
    public sealed class DefaultUniqueIdGenerator : IUniqueIdGenerator
    {
        /// <summary>
        /// Generates a new, unique identifier.
        /// </summary>
        /// <returns>The new <see cref="Guid">globaly unique identifier</see>.</returns>
        /// <remarks>This method returns the value of <see cref="Guid.NewGuid"/>.</remarks>
        public Guid NewId() => Guid.NewGuid();
    }
}