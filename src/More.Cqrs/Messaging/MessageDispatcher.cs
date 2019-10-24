// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Sagas;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    sealed class MessageDispatcher
    {
        readonly MessageDispatcher<ICommand> commands;
        readonly MessageDispatcher<IEvent> events;

        internal MessageDispatcher( IMessageBusConfiguration configuration, ISagaActivator sagaActivator )
        {
            commands = new CommandDispatcher( configuration, sagaActivator );
            events = new EventDispatcher( configuration, sagaActivator );
        }

        internal Task Dispatch( IMessage message, IMessageContext context, CancellationToken cancellationToken )
        {
            Correlation.CurrentId = message.CorrelationId;

            switch ( message )
            {
                case ICommand command:
                    return commands.Dispatch( command, context, cancellationToken );
                case IEvent @event:
                    return events.Dispatch( @event, context, cancellationToken );
            }

            return CompletedTask;
        }
    }
}