﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using More.Domain.Messaging;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    sealed class SnapshotSerializer
    {
        readonly Type keyType;
        readonly TypeInfo eventTypeInfo = typeof( IEvent ).GetTypeInfo();
        readonly TypeInfo snapshotOfTTypeInfo;
        readonly SqlEventStoreConfiguration configuration;
        readonly ConcurrentDictionary<string, IAbstractDeserializerFactory> factories = new ConcurrentDictionary<string, IAbstractDeserializerFactory>();

        internal SnapshotSerializer( Type keyType, SqlEventStoreConfiguration configuration )
        {
            this.keyType = keyType;
            this.configuration = configuration;
            snapshotOfTTypeInfo = typeof( ISnapshot<> ).MakeGenericType( keyType ).GetTypeInfo();
        }

        internal IAsyncEnumerable<IEvent> Deserialize<TKey>( IAsyncEnumerable<IEvent> events, SqlSnapshotDescriptor<TKey> snapshotDescriptor ) where TKey : notnull
        {
            var abstractFactory = factories.GetOrAdd( snapshotDescriptor.SnapshotType, NewFactory );
            var deserialize = abstractFactory.NewDeserializer( snapshotDescriptor.SnapshotType );
            return deserialize( events, snapshotDescriptor.Snapshot );
        }

        IAbstractDeserializerFactory NewFactory( string snapshotTypeName )
        {
            var snapshotType = ResolveAndValidateType( snapshotTypeName );
            var factoryType = typeof( AbstractDeserializerFactory<,> ).MakeGenericType( keyType, snapshotType );
            var factory = Activator.CreateInstance( factoryType, configuration )!;
            return (IAbstractDeserializerFactory) factory;
        }

        Type ResolveAndValidateType( string snapshotTypeName )
        {
            var snapshotType = configuration.MessageTypeResolver.ResolveType( snapshotTypeName, 1 );
            var typeInfo = snapshotType.GetTypeInfo();

            if ( typeInfo.IsValueType || !eventTypeInfo.IsAssignableFrom( typeInfo ) || !snapshotOfTTypeInfo.IsAssignableFrom( typeInfo ) )
            {
                throw new InvalidOperationException( SR.InvalidSnapshotType.FormatDefault( typeInfo.Name ) );
            }

            return snapshotType;
        }

        interface IAbstractDeserializerFactory
        {
            Func<IAsyncEnumerable<IEvent>, Stream, IAsyncEnumerable<IEvent>> NewDeserializer( string snapshotTypeName );
        }

#pragma warning disable CA1812
        sealed class AbstractDeserializerFactory<TKey, TSnapshot> : IAbstractDeserializerFactory
            where TKey : notnull
            where TSnapshot : class, IEvent, ISnapshot<TKey>
#pragma warning restore CA1812
        {
            const int Revision = 1;
            readonly ISqlMessageSerializer<TSnapshot> serializer;

            public AbstractDeserializerFactory( SqlEventStoreConfiguration configuration ) => serializer = configuration.NewMessageSerializer<TSnapshot>();

            public Func<IAsyncEnumerable<IEvent>, Stream, IAsyncEnumerable<IEvent>> NewDeserializer( string snapshotTypeName ) =>
                ( events, snapshot ) => new AsyncSnapshotEventStream<TKey, TSnapshot>( events, serializer.Deserialize( snapshotTypeName, Revision, snapshot ) );
        }
    }
}