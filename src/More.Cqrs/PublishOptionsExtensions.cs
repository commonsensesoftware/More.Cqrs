// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Options;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides extension methods for the <see cref="PublishOptions"/> class.
    /// </summary>
    public static class PublishOptionsExtensions
    {
        /// <summary>
        /// Configures an option to delay the delivery of a message until the specified date and time.
        /// </summary>
        /// <param name="options">The extended <see cref="PublishOptions"/>.</param>
        /// <param name="when">The <see cref="DateTimeOffset">date and time</see> when the message should be delivered.</param>
        /// <returns>The original <paramref name="options"/> instance.</returns>
        public static PublishOptions DoNotDeliverBefore( this PublishOptions options, DateTimeOffset when )
        {
            Arg.NotNull( options, nameof( options ) );
            Contract.Ensures( Contract.Result<PublishOptions>() != null );

            options.Add( new DoNotDeliverBefore( when.UtcDateTime ) );
            return options;
        }

        /// <summary>
        /// Configures an option to delay the delivery of a message until the specified time has elapsed.
        /// </summary>
        /// <param name="options">The extended <see cref="PublishOptions"/>.</param>
        /// <param name="delay">The <see cref="TimeSpan">amount of time</see> to delay delivery by.</param>
        /// <returns>The original <paramref name="options"/> instance.</returns>
        public static PublishOptions DelayDeliveryBy( this PublishOptions options, TimeSpan delay )
        {
            Arg.NotNull( options, nameof( options ) );
            Contract.Ensures( Contract.Result<PublishOptions>() != null );

            options.Add( new DelayDeliveryBy( delay ) );
            return options;
        }
    }
}