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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Linq.Expressions.Expression;
    using static System.Threading.Tasks.Task;

    sealed class EventDispatcher : MessageDispatcher<IEvent>
    {
        static readonly MethodInfo DispatchOfT = typeof( EventDispatcher ).GetRuntimeMethods().Single( m => m.Name == nameof( DispatchEvent ) );
        readonly IEventReceiverRegistrar eventReceivers;
        readonly ConcurrentDictionary<Type, Func<IEvent, IMessageContext, CancellationToken, Task>> dispatchers =
            new ConcurrentDictionary<Type, Func<IEvent, IMessageContext, CancellationToken, Task>>();

        internal EventDispatcher( IMessageBusConfiguration configuration, ISagaActivator saga )
            : base( configuration, saga ) => eventReceivers = configuration.EventReceivers;

        public override Task Dispatch( IEvent message, IMessageContext context, CancellationToken cancellationToken ) =>
            dispatchers.GetOrAdd( message.GetType(), NewDispatcher )( message, context, cancellationToken );

        Task DispatchEvent<TEvent>( TEvent @event, IMessageContext context, CancellationToken cancellationToken ) where TEvent : class, IEvent
        {
            var handlers = eventReceivers.ResolveFor( @event );
            var tasks = new List<Task>();

            foreach ( var handler in handlers )
            {
                if ( handler.IsSaga() )
                {
                    tasks.Add( StartOrResumeSaga( handler, @event, cancellationToken ) );
                }
                else
                {
                    tasks.Add( handler.Receive( @event, context, cancellationToken ).AsTask() );
                }
            }

            return WhenAll( tasks );
        }

        Func<IEvent, IMessageContext, CancellationToken, Task> NewDispatcher( Type eventType )
        {
            var @event = Parameter( typeof( IEvent ), "event" );
            var context = Parameter( typeof( IMessageContext ), "context" );
            var cancellationToken = Parameter( typeof( CancellationToken ), "cancellationToken" );
            var @this = Constant( this );
            var method = DispatchOfT.MakeGenericMethod( eventType );
            var body = Call( @this, method, Convert( @event, eventType ), context, cancellationToken );
            var lambda = Lambda<Func<IEvent, IMessageContext, CancellationToken, Task>>( body, @event, context, cancellationToken );

            return lambda.Compile();
        }

        async Task StartOrResumeSaga<TEvent>( IReceiveEvent<TEvent> receiver, TEvent @event, CancellationToken cancellationToken ) where TEvent : class, IEvent
        {
            var instance = await Saga.Activate( receiver, @event, cancellationToken ).ConfigureAwait( false );

            if ( instance == null || instance.NotFound )
            {
                // TODO: what should we do when we don't find a saga instance? anything?
                return;
            }

            var context = new SagaMessageContext( Configuration );

            await receiver.Receive( @event, context, cancellationToken ).ConfigureAwait( false );
            await TransitionState( instance, instance.Data.Version, context, cancellationToken ).ConfigureAwait( false );
        }
    }
}