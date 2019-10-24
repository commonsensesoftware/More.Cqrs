﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain.Events
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of a <see cref="IEvent">event</see> registrar.
    /// </summary>
    public interface IEventReceiverRegistrar
    {
        /// <summary>
        /// Registers a factory method used to resolve and activate an event receiver for a given event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="receiverActivator">The factory <see cref="Func{T}">method</see> used to activate the <see cref="IReceiveEvent{T}">event receiver</see>.</param>
        void Register<TEvent>( Func<IReceiveEvent<TEvent>> receiverActivator ) where TEvent : notnull, IEvent;

        /// <summary>
        /// Resolves the event receivers for the specified event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="event">The event to resolve the receivers for.</param>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of <see cref="IReceiveEvent{T}">event receivers</see>.</returns>
        IEnumerable<IReceiveEvent<TEvent>> ResolveFor<TEvent>( TEvent @event ) where TEvent : notnull, IEvent;
    }
}