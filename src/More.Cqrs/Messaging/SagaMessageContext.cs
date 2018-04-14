// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    sealed class SagaMessageContext : IMessageContext
    {
        readonly IServiceProvider serviceProvider;
        readonly List<IMessageDescriptor> messages = new List<IMessageDescriptor>();

        internal SagaMessageContext( IServiceProvider serviceProvider, CancellationToken cancellationToken )
        {
            Contract.Requires( serviceProvider != null );

            this.serviceProvider = serviceProvider;
            CancellationToken = cancellationToken;
        }

        internal IReadOnlyList<IMessageDescriptor> Messages => messages;

        public CancellationToken CancellationToken { get; }

        public Task Publish( IEvent @event, PublishOptions options ) => QueueMessage( @event, options );

        public Task Send( ICommand command, SendOptions options ) => QueueMessage( command, options );

        public object GetService( Type serviceType ) => serviceProvider.GetService( serviceType );

        Task QueueMessage( IMessage message, IOptions options )
        {
            Contract.Requires( message != null );
            Contract.Requires( options != null );

            messages.Add( message.GetDescriptor( options ) );
            return CompletedTask;
        }
    }
}