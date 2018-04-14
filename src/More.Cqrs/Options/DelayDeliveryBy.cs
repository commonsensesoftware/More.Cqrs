// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Options
{
    using System;

    /// <summary>
    /// Represents an option to delay the delivery of a message by a specified amount of time.
    /// </summary>
    public class DelayDeliveryBy : IDeliveryOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelayDeliveryBy"/> class.
        /// </summary>
        /// <param name="delay">The <see cref="TimeSpan">amount of time</see> to delay the message delivery by.</param>
        public DelayDeliveryBy( TimeSpan delay ) => Delay = delay;

        /// <summary>
        /// Gets the amount of time to delay the delivery of a message by.
        /// </summary>
        /// <value>The <see cref="TimeSpan">amount of time</see> to delay the message delivery by.</value>
        public TimeSpan Delay { get; }
    }
}