// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;

    /// <summary>
    /// Defines the behavior of an ISAM database message serializer factory.
    /// </summary>
    public interface IIsamMessageSerializerFactory
    {
        /// <summary>
        /// Creates a new message serializer for the specified type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <returns>A new <see cref="IIsamMessageSerializer{TMessage}">message serializer</see>.</returns>
        IIsamMessageSerializer<TMessage> NewSerializer<TMessage>() where TMessage : class;
    }
}