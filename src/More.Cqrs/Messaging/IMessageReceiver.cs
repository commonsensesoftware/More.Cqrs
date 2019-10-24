// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;

    /// <summary>
    /// Defines the behavior of a message receiver.
    /// </summary>
    public interface IMessageReceiver : IObservable<IMessageDescriptor>
    {
    }
}