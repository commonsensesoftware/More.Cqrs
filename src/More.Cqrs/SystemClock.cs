// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Diagnostics;
    using static System.DateTime;

    /// <summary>
    /// Represents a clock based on the current computer's system date and time.
    /// </summary>
    [DebuggerDisplay( "{Now}" )]
    public sealed class SystemClock : IClock
    {
        /// <summary>
        /// Gets the clock's current time.
        /// </summary>
        /// <value>The clock's current <see cref="DateTimeOffset">date and time</see>.
        /// The value is always <see cref="DateTimeOffset.UtcNow"/>.</value>
        public DateTimeOffset Now => UtcNow;
    }
}