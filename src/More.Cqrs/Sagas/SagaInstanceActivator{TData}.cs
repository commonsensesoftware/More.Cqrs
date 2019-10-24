// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Activator;

    /// <summary>
    /// Represents an object that activates <see cref="ISagaInstance">saga instances</see>.
    /// </summary>
    /// <typeparam name="TData">The type of saga data.</typeparam>
    public class SagaInstanceActivator<TData> : ISagaInstanceActivator where TData : class, ISagaData
    {
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaInstanceActivator{TData}"/> class.
        /// </summary>
        public SagaInstanceActivator() => serviceProvider = ServiceProvider.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaInstanceActivator{TData}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The underlaying <see cref="IServiceProvider">service provider</see>.</param>
        public SagaInstanceActivator( IServiceProvider serviceProvider ) => this.serviceProvider = serviceProvider;

        /// <summary>
        /// Activates a saga using the provided metadata and clock.
        /// </summary>
        /// <param name="metadata">The <see cref="SagaMetadata">metadata</see> for the active saga.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> associated with the active saga.</param>
        /// <returns>The activated <see cref="ISagaInstance">saga instance</see>.</returns>
        public ISagaInstance Activate( SagaMetadata metadata, IClock clock ) =>
            new SagaInstance<TData>( (ISaga<TData>) serviceProvider.GetService( typeof( ISaga<TData> ) ), metadata, clock );

        /// <summary>
        /// Activates a saga from the specified command handler using the provided metadata and clock.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="commandHandler">The <see cref="IHandleCommand{TCommand}">command handler</see> used to activate the saga.</param>
        /// <param name="metadata">The <see cref="SagaMetadata">metadata</see> for the active saga.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> associated with the active saga.</param>
        /// <returns>The activated <see cref="ISagaInstance">saga instance</see>.</returns>
        public ISagaInstance Activate<TCommand>( IHandleCommand<TCommand> commandHandler, SagaMetadata metadata, IClock clock ) where TCommand : notnull, ICommand =>
            new SagaInstance<TData>( (ISaga<TData>) commandHandler, metadata, clock );

        /// <summary>
        /// Activates a saga from the specified event receiver using the provided metadata and clock.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventReceiver">The <see cref="IReceiveEvent{TEvent}">event receiver</see> used to activate the saga.</param>
        /// <param name="metadata">The <see cref="SagaMetadata">metadata</see> for the active saga.</param>
        /// <param name="clock">The <see cref="IClock">clock</see> associated with the active saga.</param>
        /// <returns>The activated <see cref="ISagaInstance">saga instance</see>.</returns>
        public ISagaInstance Activate<TEvent>( IReceiveEvent<TEvent> eventReceiver, SagaMetadata metadata, IClock clock ) where TEvent : notnull, IEvent =>
            new SagaInstance<TData>( (ISaga<TData>) eventReceiver, metadata, clock );

        /// <summary>
        /// Gets the data for the active saga instance using the specified saga storage and message.
        /// </summary>
        /// <param name="instance">The active <see cref="ISagaInstance">saga instance</see> to get the data for.</param>
        /// <param name="store">The <see cref="IStoreSagaData">saga storage</see> used to retrieve the data.</param>
        /// <param name="message">The correlated message used to retrieve the data.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="SagaSearchResult">search result</see>.</returns>
        public async Task<SagaSearchResult> GetData( ISagaInstance instance, IStoreSagaData store, object message, CancellationToken cancellationToken )
        {
            var data = default( ISagaData );

            if ( instance.SagaId != default )
            {
                data = await store.Retrieve<TData>( instance.SagaId, cancellationToken ).ConfigureAwait( false );
            }

            if ( data != null )
            {
                return new SagaSearchResult( data );
            }

            var metadata = instance.Metadata;
            var messageType = message.GetType();

            if ( !metadata.TryGetSearchMethod( messageType.FullName!, out var searchMethod ) )
            {
                return new SagaSearchResult();
            }

            var searcher = NewSearcher( searchMethod, store );

            return await searcher.Search( searchMethod, message, cancellationToken ).ConfigureAwait( false );
        }

        /// <summary>
        /// Creates a new searcher using the specified search method and saga storage.
        /// </summary>
        /// <param name="searchMethod">The <see cref="SagaSearchMethod">search method</see> to create a searcher for.</param>
        /// <param name="store">The associated <see cref="IStoreSagaData">saga storage</see>.</param>
        /// <returns>A new <see cref="ISearchForSaga">searcher</see> instance.</returns>
        protected virtual ISearchForSaga NewSearcher( SagaSearchMethod searchMethod, IStoreSagaData store ) =>
            (ISearchForSaga) ( serviceProvider.GetService( searchMethod.Type ) ?? CreateInstance( searchMethod.Type, store ) )!;
    }
}