// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    /// <summary>
    /// Represents the base implementation for an aggregate that uses a globally unique identifier (GUID) for its key.
    /// </summary>
    public abstract class Aggregate : Aggregate<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Aggregate"/> class.
        /// </summary>
        protected Aggregate() { }
    }
}