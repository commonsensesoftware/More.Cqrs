// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using static Newtonsoft.Json.Linq.JObject;

    sealed class JsonMessageSerializer<TMessage> where TMessage : IMessage
    {
        static readonly TypeInfo MessageTypeInfo = typeof( TMessage ).GetTypeInfo();
        static readonly ConcurrentDictionary<(string TypeName, int Revision), Func<JObject, TMessage>> deserializers =
            new ConcurrentDictionary<(string TypeName, int Revision), Func<JObject, TMessage>>();
        readonly IMessageTypeResolver messageTypeResolver;
        readonly JsonSerializer serializer;

        internal JsonMessageSerializer( IMessageTypeResolver messageTypeResolver, JsonSerializer serializer )
        {
            this.messageTypeResolver = messageTypeResolver;
            this.serializer = serializer;
        }

        internal JObject Serialize( TMessage message ) => FromObject( message, serializer );

        internal TMessage Deserialize( string messageTypeName, int revision, JObject message ) =>
            deserializers.GetOrAdd( (TypeName: messageTypeName, Revision: revision), NewDeserializer )( message );

        Func<JObject, TMessage> NewDeserializer( (string TypeName, int Revision) entry )
        {
            var objectType = messageTypeResolver.ResolveType( entry.TypeName, entry.Revision );

            if ( !MessageTypeInfo.IsAssignableFrom( objectType.GetTypeInfo() ) )
            {
                throw new InvalidOperationException( SR.CannotDeserializeMessageType.FormatDefault( entry.TypeName, MessageTypeInfo.Name ) );
            }

            return json => json.ToObject<TMessage>( serializer );
        }
    }
}