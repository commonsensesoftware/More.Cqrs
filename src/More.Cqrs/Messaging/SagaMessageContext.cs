// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    sealed class SagaMessageContext : IMessageContext
    {
        readonly IServiceProvider serviceProvider;
        readonly List<IMessageDescriptor> messages = new List<IMessageDescriptor>();

        internal SagaMessageContext( IServiceProvider serviceProvider ) =>
            this.serviceProvider = serviceProvider;

        internal IReadOnlyList<IMessageDescriptor> Messages => messages;

        public Task Publish( IEvent @event, PublishOptions options ) => QueueMessage( @event, options );

        public Task Send( ICommand command, SendOptions options ) => QueueMessage( command, options );

        public object GetService( Type serviceType ) => serviceProvider.GetService( serviceType );

        Task QueueMessage( IMessage message, IOptions options )
        {
            messages.Add( message.GetDescriptor( options ) );
            return CompletedTask;
        }
    }
}