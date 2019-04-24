// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Options;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides extension methods for the <see cref="IMessage"/> interface.
    /// </summary>
    public static class IMessageExtensions
    {
        /// <summary>
        /// Creates and returns a descriptor for the current message.
        /// </summary>
        /// <param name="message">The <see cref="IMessage">message</see> to get a descriptor for.</param>
        /// <returns>A new <see cref="IMessageDescriptor">message descriptor</see> with default <see cref="IOptions">options</see>.</returns>
        public static IMessageDescriptor GetDescriptor( this IMessage message )
        {
            Arg.NotNull( message, nameof( message ) );
            Contract.Ensures( Contract.Result<IMessageDescriptor>() != null );
            return message.GetDescriptor( DefaultOptions.None );
        }
    }
}