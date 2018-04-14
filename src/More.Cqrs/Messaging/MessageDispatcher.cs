// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Sagas;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    sealed class MessageDispatcher
    {
        readonly MessageDispatcher<ICommand> commands;
        readonly MessageDispatcher<IEvent> events;

        internal MessageDispatcher( IMessageBusConfiguration configuration, ISagaActivator sagaActivator )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( sagaActivator != null );

            commands = new CommandDispatcher( configuration, sagaActivator );
            events = new EventDispatcher( configuration, sagaActivator );
        }

        internal Task Dispatch( IMessage message, IMessageContext context )
        {
            Contract.Requires( message != null );
            Contract.Requires( context != null );

            Correlation.CurrentId = message.CorrelationId;

            switch ( message )
            {
                case ICommand command:
                    return commands.Dispatch( command, context);
                case IEvent @event:
                    return events.Dispatch( @event, context );
            }

            return CompletedTask;
        }
    }
}