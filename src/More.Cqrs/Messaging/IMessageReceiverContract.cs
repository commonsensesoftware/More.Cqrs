// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Diagnostics.Contracts;

    abstract class IMessageReceiverContract : IMessageReceiver
    {
        IDisposable IObservable<IMessageDescriptor>.Subscribe( IObserver<IMessageDescriptor> observer )
        {
            Contract.Requires<ArgumentNullException>( observer != null );
            Contract.Ensures( Contract.Result<IDisposable>() != null );
            return null;
        }
    }
}