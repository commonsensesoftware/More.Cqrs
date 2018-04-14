// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of a message receiver.
    /// </summary>
    [ContractClass( typeof( IMessageReceiverContract ) )]
    public interface IMessageReceiver : IObservable<IMessageDescriptor>
    {
    }
}