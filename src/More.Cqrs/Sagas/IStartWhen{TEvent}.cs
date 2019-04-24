// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Events;
    using System;

    /// <summary>
    /// Defines the behavior of an event that starts a saga.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    public interface IStartWhen<in TEvent> : IReceiveEvent<TEvent> where TEvent : IEvent
    {
    }
}