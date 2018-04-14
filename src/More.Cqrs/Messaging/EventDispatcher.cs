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
        readonly ConcurrentDictionary<Type, Func<IEvent, IMessageContext, Task>> dispatchers = new ConcurrentDictionary<Type, Func<IEvent, IMessageContext, Task>>();

        internal EventDispatcher( IMessageBusConfiguration configuration, ISagaActivator saga )
            : base( configuration, saga ) => eventReceivers = configuration.EventReceivers;

        public override Task Dispatch( IEvent message, IMessageContext context ) => dispatchers.GetOrAdd( message.GetType(), NewDispatcher )( message, context );

        Task DispatchEvent<TEvent>( TEvent @event, IMessageContext context ) where TEvent : class, IEvent
        {
            var handlers = eventReceivers.ResolveFor( @event );
            var tasks = new List<Task>();

            foreach ( var handler in handlers )
            {
                if ( handler.IsSaga() )
                {
                    tasks.Add( StartOrResumeSaga( handler, @event, context.CancellationToken ) );
                }
                else
                {
                    tasks.Add( handler.Receive( @event, context ) );
                }
            }

            return WhenAll( tasks );
        }

        Func<IEvent, IMessageContext, Task> NewDispatcher( Type eventType )
        {
            var @event = Parameter( typeof( IEvent ), "event" );
            var context = Parameter( typeof( IMessageContext ), "context" );
            var @this = Constant( this );
            var method = DispatchOfT.MakeGenericMethod( eventType );
            var body = Call( @this, method, Convert( @event, eventType ), context );
            var lambda = Lambda<Func<IEvent, IMessageContext, Task>>( body, @event, context );

            return lambda.Compile();
        }

        async Task StartOrResumeSaga<TEvent>( IReceiveEvent<TEvent> receiver, TEvent @event, CancellationToken cancellationToken ) where TEvent : class, IEvent
        {
            var instance = await Saga.Activate( receiver, @event, cancellationToken ).ConfigureAwait( false );

            if ( instance?.NotFound == true )
            {
                // TODO: what should we do when we don't find a saga instance? anything?
                return;
            }

            var context = new SagaMessageContext( Configuration, cancellationToken );

            await receiver.Receive( @event, context ).ConfigureAwait( false );
            await TransitionState( instance, instance.Data.Version, context ).ConfigureAwait( false );
        }
    }
}