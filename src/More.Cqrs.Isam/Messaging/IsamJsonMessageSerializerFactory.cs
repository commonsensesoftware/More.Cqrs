// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using static Newtonsoft.Json.JsonSerializer;

    /// <summary>
    /// Represents an ISAM database message serializer factory that creates serializers that use the JSON format.
    /// </summary>
    public sealed class IsamJsonMessageSerializerFactory : IIsamMessageSerializerFactory
    {
        readonly IMessageTypeResolver messageTypeResolver;
        readonly JsonSerializerSettings serializerSettings;
        readonly ConcurrentDictionary<Type, object> serializers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamJsonMessageSerializerFactory"/> class.
        /// </summary>
        /// <param name="messageTypeResolver">The <see cref="IMessageTypeResolver">message type resolver</see> used by the factory.</param>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings">JSON serialization settings</see> used by the factory.</param>
        public IsamJsonMessageSerializerFactory( IMessageTypeResolver messageTypeResolver, JsonSerializerSettings serializerSettings )
        {
            Arg.NotNull( messageTypeResolver, nameof( messageTypeResolver ) );
            Arg.NotNull( serializerSettings, nameof( serializerSettings ) );

            this.messageTypeResolver = messageTypeResolver;
            this.serializerSettings = serializerSettings;
        }

        /// <summary>
        /// Creates a new message serializer for the specified type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <returns>A new <see cref="IIsamMessageSerializer{TMessage}">message serializer</see>.</returns>
        /// <remarks>This method always returns an instance of the <see cref="IsamJsonMessageSerializer{TMessage}"/> class.</remarks>
        public IIsamMessageSerializer<TMessage> NewSerializer<TMessage>() where TMessage : class
        {
            var serializer = serializers.GetOrAdd( typeof(TMessage), key => new IsamJsonMessageSerializer<TMessage>( messageTypeResolver, Create( serializerSettings ) ) );
            return (IIsamMessageSerializer<TMessage>) serializer;
        }
    }
}