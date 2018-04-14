// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Options;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides extension methods for the <see cref="SendOptions"/> class.
    /// </summary>
    public static class SendOptionsExtensions
    {
        /// <summary>
        /// Configures an option to delay the delivery of a message until the specified date and time.
        /// </summary>
        /// <param name="options">The extended <see cref="SendOptions"/>.</param>
        /// <param name="when">The <see cref="DateTimeOffset">date and time</see> when the message should be delivered.</param>
        /// <returns>The original <paramref name="options"/> instance.</returns>
        public static SendOptions DoNotDeliverBefore( this SendOptions options, DateTimeOffset when )
        {
            Arg.NotNull( options, nameof( options ) );
            Contract.Ensures( Contract.Result<SendOptions>() != null );

            options.Add( new DoNotDeliverBefore( when.UtcDateTime ) );
            return options;
        }

        /// <summary>
        /// Configures an option to delay the delivery of a message until the specified time has elapsed.
        /// </summary>
        /// <param name="options">The extended <see cref="SendOptions"/>.</param>
        /// <param name="delay">The <see cref="TimeSpan">amount of time</see> to delay delivery by.</param>
        /// <returns>The original <paramref name="options"/> instance.</returns>
        public static SendOptions DelayDeliveryBy( this SendOptions options, TimeSpan delay )
        {
            Arg.NotNull( options, nameof( options ) );
            Contract.Ensures( Contract.Result<SendOptions>() != null );

            options.Add( new DelayDeliveryBy( delay ) );
            return options;
        }
    }
}