// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Persistence;
    using System;

    sealed class InMemoryMessageBusConfiguration : IMessageBusConfiguration
    {
        readonly IMessageBusConfiguration inner;

        internal InMemoryMessageBusConfiguration( IMessageBusConfiguration configuration )
            : this( configuration, new PendingOperations() ) { }

        internal InMemoryMessageBusConfiguration( IMessageBusConfiguration configuration, PendingOperations pendingOperations )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( pendingOperations, nameof( pendingOperations ) );

            inner = configuration;
            PendingOperations = pendingOperations;
        }

        public PendingOperations PendingOperations { get; }

        public IClock Clock => inner.Clock;

        public ICommandHandlerRegistrar CommandHandlers => inner.CommandHandlers;

        public IEventReceiverRegistrar EventReceivers => inner.EventReceivers;

        public IMapPersistence Persistence => inner.Persistence;

        public IMessageReceiver MessageReceiver => inner.MessageReceiver;

        public IMessageSender MessageSender => inner.MessageSender;

        public SagaConfiguration Sagas => inner.Sagas;

        public object GetService( Type serviceType ) => inner.GetService( serviceType );

        public IUniqueIdGenerator UniqueIdGenerator => inner.UniqueIdGenerator;
    }
}