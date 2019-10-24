// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;

    /// <summary>
    /// Represents a predicate which filters events in between a date range.
    /// </summary>
    /// <typeparam name="TKey">The type of event key.</typeparam>
    public class EventDateRangePredicate<TKey> : IEventPredicate<TKey> where TKey : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventDateRangePredicate{TKey}"/> class.
        /// </summary>
        /// <param name="from">The <see cref="DateTimeOffset">date and time</see> of the first event.</param>
        /// <param name="to">The optional <see cref="DateTimeOffset">date and time</see> of the last event.</param>
        public EventDateRangePredicate( DateTimeOffset from, DateTimeOffset? to )
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDateRangePredicate{TKey}"/> class.
        /// </summary>
        /// <param name="from">The optional <see cref="DateTimeOffset">date and time</see> of the first event.</param>
        /// <param name="to">The <see cref="DateTimeOffset">date and time</see> of the last event.</param>
        public EventDateRangePredicate( DateTimeOffset? from, DateTimeOffset to )
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Gets the date and time of the first event.
        /// </summary>
        /// <value>The <see cref="DateTimeOffset">date and time</see> of the first event.</value>
        public DateTimeOffset? From { get; }

        /// <summary>
        /// Gets the date and time of the last event.
        /// </summary>
        /// <value>The <see cref="DateTimeOffset">date and time</see> of the last event.</value>
        public DateTimeOffset? To { get; }
    }
}