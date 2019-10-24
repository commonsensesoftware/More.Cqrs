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
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Linq.Expressions.Expression;

    sealed class CommandDispatcher : MessageDispatcher<ICommand>
    {
        static readonly MethodInfo DispatchOfT = typeof( CommandDispatcher ).GetRuntimeMethods().Single( m => m.Name == nameof( DispatchCommand ) );
        readonly ICommandHandlerRegistrar commandHandlers;
        readonly ConcurrentDictionary<Type, Func<ICommand, IMessageContext, CancellationToken, Task>> dispatchers =
            new ConcurrentDictionary<Type, Func<ICommand, IMessageContext, CancellationToken, Task>>();

        internal CommandDispatcher( IMessageBusConfiguration configuration, ISagaActivator saga )
            : base( configuration, saga ) => commandHandlers = configuration.CommandHandlers;

        public override Task Dispatch( ICommand message, IMessageContext context, CancellationToken cancellationToken ) =>
            dispatchers.GetOrAdd( message.GetType(), NewDispatcher )( message, context, cancellationToken );

        Task DispatchCommand<TCommand>( TCommand command, IMessageContext context, CancellationToken cancellationToken ) where TCommand : class, ICommand
        {
            var handler = commandHandlers.ResolveFor( command );

            if ( handler.IsSaga() )
            {
                return StartOrResumeSaga( handler, command, cancellationToken );
            }

            return handler.Handle( command, context, cancellationToken ).AsTask();
        }

        Func<ICommand, IMessageContext, CancellationToken, Task> NewDispatcher( Type commandType )
        {
            var command = Parameter( typeof( ICommand ), "command" );
            var context = Parameter( typeof( IMessageContext ), "context" );
            var cancellationToken = Parameter( typeof( CancellationToken ), "cancellationToken" );
            var @this = Constant( this );
            var method = DispatchOfT.MakeGenericMethod( commandType );
            var body = Call( @this, method, Convert( command, commandType ), context, cancellationToken );
            var lambda = Lambda<Func<ICommand, IMessageContext, CancellationToken, Task>>( body, command, context, cancellationToken );

            return lambda.Compile();
        }

        async Task StartOrResumeSaga<TCommand>( IHandleCommand<TCommand> handler, TCommand command, CancellationToken cancellationToken ) where TCommand : class, ICommand
        {
            var instance = await Saga.Activate( handler, command, cancellationToken ).ConfigureAwait( false );

            if ( instance == null || instance.NotFound )
            {
                // TODO: what should we do when we don't find a saga instance? anything?
                return;
            }

            var context = new SagaMessageContext( Configuration );

            await handler.Handle( command, context, cancellationToken ).ConfigureAwait( false );
            await TransitionState( instance, command.ExpectedVersion, context, cancellationToken ).ConfigureAwait( false );
        }
    }
}