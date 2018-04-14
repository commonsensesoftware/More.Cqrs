// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// Defines the behavior of a serializer for SQL database message stores.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to serialize and deserialize.</typeparam>
    [ContractClass( typeof( ISqlMessageSerializerContract<> ) )]
    public interface ISqlMessageSerializer<TMessage> where TMessage : class
    {
        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <returns>A <see cref="Stream">stream</see> containing the serialized message.</returns>
        Stream Serialize( TMessage message );

        /// <summary>
        /// Deserializes the specified message.
        /// </summary>
        /// <param name="messageType">The qualified type name of the message to deserialize.</param>
        /// <param name="revision">The revision of the message being deserialized.</param>
        /// <param name="message">The <see cref="Stream">stream</see> containing the message to be deserialized.</param>
        /// <returns>The deserialized <typeparamref name="TMessage">message</typeparamref>.</returns>
        TMessage Deserialize( string messageType, int revision, Stream message );
    }
}