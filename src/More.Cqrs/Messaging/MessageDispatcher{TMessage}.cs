// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Persistence;
    using More.Domain.Sagas;
    using System.Threading;
    using System.Threading.Tasks;

    abstract class MessageDispatcher<TMessage> where TMessage : IMessage
    {
        protected MessageDispatcher( IMessageBusConfiguration configuration, ISagaActivator saga )
        {
            Configuration = configuration;
            Saga = saga;
        }

        protected IMessageBusConfiguration Configuration { get; }

        protected ISagaActivator Saga { get; }

        protected async Task TransitionState( ISagaInstance instance, int expectedVersion, SagaMessageContext context, CancellationToken cancellationToken )
        {
            instance.ValidateChanges();

            var logName = instance.Metadata.SagaType.Name;
            var persistence = Configuration.Persistence.Map( logName );
            var commit = new Commit()
            {
                Id = instance.SagaId,
                Version = expectedVersion + 1,
                Saga = instance,
            };

            foreach ( var @event in instance.UncommittedEvents )
            {
                commit.Messages.Add( @event.GetDescriptor() );
                commit.Events.Add( @event );
            }

            foreach ( var message in context.Messages )
            {
                commit.Messages.Add( message );
            }

            await persistence.Persist( commit, cancellationToken ).ConfigureAwait( false );

            instance.AcceptChanges();
            instance.Update();
        }

        public abstract Task Dispatch( TMessage message, IMessageContext context, CancellationToken cancellationToken );
    }
}