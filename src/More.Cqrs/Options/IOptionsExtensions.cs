// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Options
{
    using System;
    using static System.TimeSpan;

    /// <summary>
    /// Provides extension methods for the <see cref="IOptions"/> interface.
    /// </summary>
    public static class IOptionsExtensions
    {
        /// <summary>
        /// Returns the configured delivery delay.
        /// </summary>
        /// <param name="options">The extended <see cref="IOptions"/>.</param>
        /// <returns>The <see cref="TimeSpan">amount of time</see> to delay the delivery of a message.</returns>
        /// <remarks>The <see cref="DoNotDeliverBefore"></see> option is calculated by subtracting
        /// <see cref="DoNotDeliverBefore.When"/> from <see cref="DateTime.UtcNow"/>.</remarks>
        public static TimeSpan GetDeliveryDelay( this IOptions options )
        {
            Arg.NotNull( options, nameof( options ) );
            return options.GetDeliveryDelay( new SystemClock() );
        }

        /// <summary>
        /// Returns the configured delivery delay.
        /// </summary>
        /// <param name="options">The extended <see cref="IOptions"/>.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> used to perform date and time calculations.</param>
        /// <returns>The <see cref="TimeSpan">amount of time</see> to delay the delivery of a message.</returns>
        public static TimeSpan GetDeliveryDelay( this IOptions options, IClock clock )
        {
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( clock, nameof( clock ) );

            var deliveryDelay = Zero;

            foreach ( var deliveryOption in options.All<IDeliveryOption>() )
            {
                switch ( deliveryOption )
                {
                    case DoNotDeliverBefore option:
                        deliveryDelay = option.When - clock.Now;
                        break;
                    case DelayDeliveryBy option:
                        deliveryDelay = option.Delay;
                        break;
                }
            }

            return deliveryDelay;
        }

        /// <summary>
        /// Returns the configured delivery time.
        /// </summary>
        /// <param name="options">The extended <see cref="IOptions"/>.</param>
        /// <returns>The <see cref="DateTimeOffset">date and time</see> when a message should be delivered.</returns>
        /// <remarks>The <see cref="DelayDeliveryBy"></see> option is calculated by adding
        /// <see cref="DelayDeliveryBy.Delay"/> to <see cref="DateTime.UtcNow"/>.</remarks>
        public static DateTimeOffset GetDeliveryTime( this IOptions options )
        {
            Arg.NotNull( options, nameof( options ) );
            return options.GetDeliveryTime( new SystemClock() );
        }

        /// <summary>
        /// Returns the configured delivery time.
        /// </summary>
        /// <param name="options">The extended <see cref="IOptions"/>.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> used to perform date and time calculations.</param>
        /// <returns>The <see cref="DateTimeOffset">date and time</see> when a message should be delivered.</returns>
        public static DateTimeOffset GetDeliveryTime( this IOptions options, IClock clock )
        {
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( clock, nameof( clock ) );

            var deliveryTime = clock.Now;

            foreach ( var deliveryOption in options.All<IDeliveryOption>() )
            {
                switch ( deliveryOption )
                {
                    case DoNotDeliverBefore option:
                        deliveryTime = option.When;
                        break;
                    case DelayDeliveryBy option:
                        deliveryTime = deliveryTime + option.Delay;
                        break;
                }
            }

            return deliveryTime;
        }
    }
}