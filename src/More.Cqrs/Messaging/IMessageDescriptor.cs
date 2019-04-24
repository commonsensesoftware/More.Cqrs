// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Options;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of a message descriptor.
    /// </summary>
    [ContractClass( typeof( IMessageDescriptorContract ) )]
    public interface IMessageDescriptor
    {
        /// <summary>
        /// Gets the associated aggregate identifier.
        /// </summary>
        /// <value>The associated aggregate identifier.</value>
        object AggregateId { get; }

        /// <summary>
        /// Gets the unique message identifier.
        /// </summary>
        /// <value>The unique message identifier.</value>
        string MessageId { get; }

        /// <summary>
        /// Gets the qualified type name of the described message.
        /// </summary>
        /// <value>The qualified type name of the described message.</value>
        string MessageType { get; }

        /// <summary>
        /// Gets the described message.
        /// </summary>
        /// <value>The described message.</value>
        IMessage Message { get; }

        /// <summary>
        /// Gets the options associated with the descriptor.
        /// </summary>
        /// <value>The associated <see cref="IOptions">options</see>.</value>
        IOptions Options { get; }
    }
}