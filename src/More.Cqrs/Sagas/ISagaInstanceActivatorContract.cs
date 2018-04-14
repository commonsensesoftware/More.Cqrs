// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using Commands;
    using Events;
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( ISagaInstanceActivator ) )]
    abstract class ISagaInstanceActivatorContract : ISagaInstanceActivator
    {
        ISagaInstance ISagaInstanceActivator.Activate( SagaMetadata metadata, IClock clock )
        {
            Contract.Requires<ArgumentNullException>( metadata != null, nameof( metadata ) );
            Contract.Requires<ArgumentNullException>( clock != null, nameof( clock ) );
            Contract.Ensures( Contract.Result<ISagaInstance>() != null );
            return null;
        }

        ISagaInstance ISagaInstanceActivator.Activate<TCommand>( IHandleCommand<TCommand> commandHandler, SagaMetadata metadata, IClock clock )
        {
            Contract.Requires<ArgumentNullException>( commandHandler != null, nameof( commandHandler ) );
            Contract.Requires<ArgumentNullException>( metadata != null, nameof( metadata ) );
            Contract.Requires<ArgumentNullException>( clock != null, nameof( clock ) );
            Contract.Ensures( Contract.Result<ISagaInstance>() != null );
            return null;
        }

        ISagaInstance ISagaInstanceActivator.Activate<TEvent>( IReceiveEvent<TEvent> eventReceiver, SagaMetadata metadata, IClock clock )
        {
            Contract.Requires<ArgumentNullException>( eventReceiver != null, nameof( eventReceiver ) );
            Contract.Requires<ArgumentNullException>( metadata != null, nameof( metadata ) );
            Contract.Requires<ArgumentNullException>( clock != null, nameof( clock ) );
            Contract.Ensures( Contract.Result<ISagaInstance>() != null );
            return null;
        }

        Task<SagaSearchResult> ISagaInstanceActivator.GetData( ISagaInstance instance, IStoreSagaData store, object message, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( instance != null, nameof( instance ) );
            Contract.Requires<ArgumentNullException>( store != null, nameof( store ) );
            Contract.Requires<ArgumentNullException>( message != null, nameof( message ) );
            Contract.Ensures( Contract.Result<Task<SagaSearchResult>>() != null );
            return null;
        }
    }
}