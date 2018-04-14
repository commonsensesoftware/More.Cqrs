// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    abstract class IEventPublisherContract : IEventPublisher
    {
        Task IEventPublisher.Publish( IEvent @event, PublishOptions options, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( @event != null, nameof( @event ) );
            Contract.Requires<ArgumentNullException>( options != null, nameof( options ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}