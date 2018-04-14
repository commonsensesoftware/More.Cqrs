// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    [ContractClassFor( typeof( IMessageSender ) )]
    abstract class IMessageSenderContract : IMessageSender
    {
        Task IMessageSender.Send( IEnumerable<IMessageDescriptor> messages, CancellationToken cancellationToken )
        {
            Contract.Requires<ArgumentNullException>( messages != null, nameof( messages ) );
            Contract.Ensures( Contract.Result<Task>() != null );
            return null;
        }
    }
}