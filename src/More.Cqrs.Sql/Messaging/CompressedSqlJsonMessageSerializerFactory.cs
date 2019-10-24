// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using static Newtonsoft.Json.JsonSerializer;

    /// <summary>
    /// Represents a SQL database message serializer factory that creates serializers that use the JSON format and compression.
    /// </summary>
    public sealed class CompressedSqlJsonMessageSerializerFactory : ISqlMessageSerializerFactory
    {
        readonly IMessageTypeResolver messageTypeResolver;
        readonly JsonSerializerSettings serializerSettings;
        readonly ConcurrentDictionary<Type, object> serializers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedSqlJsonMessageSerializerFactory"/> class.
        /// </summary>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used by the factory.</param>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings">JSON serialization settings</see> used by the factory.</param>
        public CompressedSqlJsonMessageSerializerFactory( IMessageTypeResolver messageTypeResolver, JsonSerializerSettings serializerSettings )
        {
            this.messageTypeResolver = messageTypeResolver;
            this.serializerSettings = serializerSettings;
        }

        /// <summary>
        /// Creates a new message serializer for the specified type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <returns>A new <see cref="ISqlMessageSerializer{TMessage}">message serializer</see>.</returns>
        /// <remarks>This method always returns an instance of the <see cref="CompressedSqlJsonMessageSerializer{TMessage}"/> class.</remarks>
        public ISqlMessageSerializer<TMessage> NewSerializer<TMessage>() where TMessage : notnull
        {
            var serializer = serializers.GetOrAdd( typeof( TMessage ), key => new CompressedSqlJsonMessageSerializer<TMessage>( messageTypeResolver, Create( serializerSettings ) ) );
            return (ISqlMessageSerializer<TMessage>) serializer;
        }
    }
}