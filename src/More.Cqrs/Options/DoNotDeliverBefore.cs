// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Options
{
    using System;

    /// <summary>
    /// Represents an option to delay the delivery of a message until a specific date and time.
    /// </summary>
    public class DoNotDeliverBefore : IDeliveryOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoNotDeliverBefore"/> class.
        /// </summary>
        /// <param name="when">The <see cref="DateTime">date and time</see> when the message should be delivered after.</param>
        public DoNotDeliverBefore( DateTime when ) => When = when;

        /// <summary>
        /// Gets the date and time when the message should be delivered after.
        /// </summary>
        /// <value>The <see cref="DateTime">date and time</see> when the message should be delivered after.</value>
        public DateTime When { get; }
    }
}