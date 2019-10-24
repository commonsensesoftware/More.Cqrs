// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Sagas;
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    sealed class SagaActivator : ISagaActivator
    {
        readonly ConcurrentDictionary<Type, ISagaInstanceActivator> activators = new ConcurrentDictionary<Type, ISagaInstanceActivator>();
        readonly IMessageBusConfiguration configuration;
        readonly IClock clock;
        readonly IStoreSagaData storage;
        readonly SagaMetadataCollection sagaMetadata;

        internal SagaActivator( IMessageBusConfiguration configuration )
        {
            var sagas = configuration.Sagas;

            this.configuration = configuration;
            clock = configuration.Clock;
            storage = sagas.Storage;
            sagaMetadata = sagas.Metadata;
        }

        public ISagaInstance Activate( ISagaData data )
        {
            var sagaDataType = data.GetType();
            var metadata = sagaMetadata.FindByData( sagaDataType );
            var activator = activators.GetOrAdd( sagaDataType, CreateActivator );
            var instance = activator.Activate( metadata, clock );

            instance.AttachExisting( data );

            return instance;
        }

        public async Task<ISagaInstance> Activate<TCommand>( IHandleCommand<TCommand> handler, TCommand command, CancellationToken cancellationToken ) where TCommand : notnull, ICommand
        {
            var sagaType = handler.GetType();
            var metadata = sagaMetadata.Find( sagaType );
            var activator = activators.GetOrAdd( metadata.SagaDataType, CreateActivator );
            var instance = activator.Activate( handler, metadata, clock );
            var searchResult = await activator.GetData( instance, storage, command, cancellationToken ).ConfigureAwait( false );

            await AttachData( instance, searchResult, command, cancellationToken ).ConfigureAwait( false );

            return instance;
        }

        public async Task<ISagaInstance> Activate<TEvent>( IReceiveEvent<TEvent> receiver, TEvent @event, CancellationToken cancellationToken ) where TEvent : notnull, IEvent
        {
            var sagaType = receiver.GetType();
            var metadata = sagaMetadata.Find( sagaType );
            var activator = activators.GetOrAdd( metadata.SagaDataType, CreateActivator );
            var instance = activator.Activate( receiver, metadata, clock );
            var searchResult = await activator.GetData( instance, storage, @event, cancellationToken ).ConfigureAwait( false );

            await AttachData( instance, searchResult, @event, cancellationToken ).ConfigureAwait( false );

            return instance;
        }

        ISagaInstanceActivator CreateActivator( Type sagaDataType )
        {
            var activatorType = typeof( SagaInstanceActivator<> ).MakeGenericType( sagaDataType );
            return (ISagaInstanceActivator) Activator.CreateInstance( activatorType, configuration )!;
        }

        Task AttachData( ISagaInstance instance, SagaSearchResult searchResult, object message, CancellationToken cancellationToken )
        {
            var data = searchResult.Data;

            if ( data != null )
            {
                instance.AttachExisting( data );
                return CompletedTask;
            }

            var metadata = instance.Metadata;
            var messageType = message.GetType();

            if ( !metadata.CanStartSaga( messageType.FullName! ) )
            {
                instance.MarkAsNotFound();
                return CompletedTask;
            }

            try
            {
                data = (ISagaData) Activator.CreateInstance( metadata.SagaDataType )!;
            }
            catch ( MissingMemberException )
            {
                data = (ISagaData) FormatterServices.GetUninitializedObject( metadata.SagaDataType );
            }

            var property = metadata.CorrelationProperty;

            if ( property != null && searchResult.Properties.TryGetValue( property.Name, out object? value ) )
            {
                property.SetValue( data, value );
            }

            if ( data.Id == default )
            {
                data.Id = configuration.UniqueIdGenerator.NewId();
            }

            instance.AttachNew( data );
            return storage.Store( data, instance.CorrelationProperty, cancellationToken );
        }
    }
}