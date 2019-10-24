// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Represents a SQL database message serializer that uses the JSON format.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to serialize and deserialize.</typeparam>
    public class SqlJsonMessageSerializer<TMessage> : ISqlMessageSerializer<TMessage> where TMessage : notnull
    {
        static readonly TypeInfo MessageTypeInfo = typeof( TMessage ).GetTypeInfo();
        readonly ConcurrentDictionary<(string typeName, int revision), Func<Stream, TMessage>> deserializers =
            new ConcurrentDictionary<(string typeName, int revision), Func<Stream, TMessage>>();
        readonly IMessageTypeResolver messageTypeResolver;
        readonly JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlJsonMessageSerializer{TMessage}"/> class.
        /// </summary>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used by the message serializer.</param>
        /// <param name="jsonSerializer">The <see cref="JsonSerializer">JSON serializer</see> used by the message serializer.</param>
        public SqlJsonMessageSerializer( IMessageTypeResolver messageTypeResolver, JsonSerializer jsonSerializer )
        {
            this.messageTypeResolver = messageTypeResolver;
            serializer = jsonSerializer;
        }

        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <returns>A <see cref="Stream">stream</see> containing the serialized message.</returns>
        public virtual Stream Serialize( TMessage message )
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter( stream, leaveOpen: true );

            serializer.Serialize( writer, message, typeof( TMessage ) );
            writer.Flush();
            stream.Position = 0L;

            return stream;
        }

        /// <summary>
        /// Deserializes the specified message.
        /// </summary>
        /// <param name="messageType">The qualified type name of the message to deserialize.</param>
        /// <param name="revision">The revision of the message being deserialized.</param>
        /// <param name="message">The <see cref="Stream">stream</see> containing the message to be deserialized.</param>
        /// <returns>The deserialized <typeparamref name="TMessage">message</typeparamref>.</returns>
        public virtual TMessage Deserialize( string messageType, int revision, Stream message ) =>
            deserializers.GetOrAdd( (typeName: messageType, revision), NewDeserializer )( message );

        Func<Stream, TMessage> NewDeserializer( (string typeName, int revision) entry )
        {
            var objectType = messageTypeResolver.ResolveType( entry.typeName, entry.revision );

            if ( !MessageTypeInfo.IsAssignableFrom( objectType.GetTypeInfo() ) )
            {
                throw new InvalidOperationException( SR.CannotDeserializeMessageType.FormatDefault( entry.typeName, MessageTypeInfo.Name ) );
            }

            return stream => (TMessage) serializer.Deserialize( new StreamReader( stream ), objectType );
        }
    }
}