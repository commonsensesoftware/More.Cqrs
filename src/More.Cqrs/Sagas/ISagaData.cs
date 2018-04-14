// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;

    /// <summary>
    /// Defines the behavior for saga data.
    /// </summary>
    public interface ISagaData : ISnapshot<Guid>
    {
        /// <summary>
        /// Gets or sets the saga identifier.
        /// </summary>
        /// <value>The unique saga identifier.</value>
        new Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the version of the saga.
        /// </summary>
        /// <value>The saga version.</value>
        new int Version { get; set; }
    }
}