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
        readonly ConcurrentDictionary<Type, Func<ICommand, IMessageContext, Task>> dispatchers = new ConcurrentDictionary<Type, Func<ICommand, IMessageContext, Task>>();

        internal CommandDispatcher( IMessageBusConfiguration configuration, ISagaActivator saga )
            : base( configuration, saga ) => commandHandlers = configuration.CommandHandlers;

        public override Task Dispatch( ICommand message, IMessageContext context ) => dispatchers.GetOrAdd( message.GetType(), NewDispatcher )( message, context );

        Task DispatchCommand<TCommand>( TCommand command, IMessageContext context ) where TCommand : class, ICommand
        {
            var handler = commandHandlers.ResolveFor( command );

            if ( handler.IsSaga() )
            {
                return StartOrResumeSaga( handler, command, context.CancellationToken );
            }

            return handler.Handle( command, context );
        }

        Func<ICommand, IMessageContext, Task> NewDispatcher( Type commandType )
        {
            var command = Parameter( typeof( ICommand ), "command" );
            var context = Parameter( typeof( IMessageContext ), "context" );
            var @this = Constant( this );
            var method = DispatchOfT.MakeGenericMethod( commandType );
            var body = Call( @this, method, Convert( command, commandType ), context );
            var lambda = Lambda<Func<ICommand, IMessageContext, Task>>( body, command, context );

            return lambda.Compile();
        }

        async Task StartOrResumeSaga<TCommand>( IHandleCommand<TCommand> handler, TCommand command, CancellationToken cancellationToken ) where TCommand : class, ICommand
        {
            var instance = await Saga.Activate( handler, command, cancellationToken ).ConfigureAwait( false );

            if ( instance?.NotFound == true )
            {
                // TODO: what should we do when we don't find a saga instance? anything?
                return;
            }

            var context = new SagaMessageContext( Configuration, cancellationToken );

            await handler.Handle( command, context ).ConfigureAwait( false );
            await TransitionState( instance, command.ExpectedVersion, context ).ConfigureAwait( false );
        }
    }
}