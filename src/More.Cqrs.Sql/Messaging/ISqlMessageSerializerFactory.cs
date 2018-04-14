// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of a SQL database message serializer factory.
    /// </summary>
    [ContractClass( typeof( ISqlMessageSerializerFactoryContract ) )]
    public interface ISqlMessageSerializerFactory
    {
        /// <summary>
        /// Creates a new message serializer for the specified type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <returns>A new <see cref="ISqlMessageSerializer{TMessage}">message serializer</see>.</returns>
        ISqlMessageSerializer<TMessage> NewSerializer<TMessage>() where TMessage : class;
    }
}