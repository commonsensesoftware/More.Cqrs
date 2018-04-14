// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Options;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of a message.
    /// </summary>
    [ContractClass( typeof( IMessageContract ) )]
    public interface IMessage
    {
        /// <summary>
        /// Gets the revision of the message.
        /// </summary>
        /// <value>The message revision.</value>
        /// <remarks>The message revision can be used to handle different versions of a message over time.</remarks>
        int Revision { get; }

        /// <summary>
        /// Gets the associated correlation identifier.
        /// </summary>
        /// <value>The associated correlation identifier.</value>
        string CorrelationId { get; }

        /// <summary>
        /// Creates and returns a descriptor for the current message.
        /// </summary>
        /// <param name="options">The <see cref="IOptions">options</see> associated with the message.</param>
        /// <returns>A new <see cref="IMessageDescriptor">message descriptor</see>.</returns>
        IMessageDescriptor GetDescriptor( IOptions options );
    }
}