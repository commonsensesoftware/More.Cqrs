// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    /// <summary>
    /// Defines the behavior of a clock.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the clock's current time.
        /// </summary>
        /// <value>The clock's current <see cref="DateTimeOffset">date and time</see>.</value>
        DateTimeOffset Now { get; }
    }
}