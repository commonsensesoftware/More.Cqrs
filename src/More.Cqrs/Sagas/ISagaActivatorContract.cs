// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( ISagaActivator ) )]
    internal abstract class ISagaActivatorContract : ISagaActivator
    {
        ISagaInstance ISagaActivator.Activate( ISagaData data )
        {
            Contract.Requires<ArgumentException>( data != null, nameof( data ) );
            Contract.Ensures( Contract.Result<ISagaInstance>() != null );
            return null;
        }

        Task<ISagaInstance> ISagaActivator.Activate<TEvent>( IReceiveEvent<TEvent> eventReceiver, TEvent @event, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentException>( eventReceiver != null, nameof( eventReceiver ) );
            Contract.Requires<ArgumentException>( @event != null, nameof( @event ) );
            Contract.Ensures( Contract.Result<Task<ISagaInstance>>() != null );
            return null;
        }

        Task<ISagaInstance> ISagaActivator.Activate<TCommand>( IHandleCommand<TCommand> commandHandler, TCommand command, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentException>( commandHandler != null, nameof( commandHandler ) );
            Contract.Requires<ArgumentException>( command != null, nameof( command ) );
            Contract.Ensures( Contract.Result<Task<ISagaInstance>>() != null );
            return null;
        }
    }
}