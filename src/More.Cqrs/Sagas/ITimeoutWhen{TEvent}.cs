// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using Events;
    using System;

    /// <summary>
    /// Defines the behavior of an event that causes a saga to timeout.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    public interface ITimeoutWhen<in TEvent> : IReceiveEvent<TEvent> where TEvent : IEvent
    {
    }
}