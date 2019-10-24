// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Represents a registrar for <see cref="IEvent">events</see> in the system.
    /// </summary>
    public class EventRegistrar : IEventReceiverRegistrar
    {
        readonly IServiceProvider serviceProvider;
        readonly ConcurrentDictionary<Type, List<Delegate>> eventReceivers = new ConcurrentDictionary<Type, List<Delegate>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRegistrar"/> class.
        /// </summary>
        public EventRegistrar() => serviceProvider = ServiceProvider.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRegistrar"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider">service provider</see> used to dynamically resolve command handlers.</param>
        public EventRegistrar( IServiceProvider serviceProvider ) => this.serviceProvider = serviceProvider;

        /// <summary>
        /// Registers a factory method used to resolve and activate an event receiver for a given event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="receiverActivator">The factory <see cref="Func{T}">method</see> used to activate the <see cref="IReceiveEvent{T}">event receiver</see>.</param>
        public virtual void Register<TEvent>( Func<IReceiveEvent<TEvent>> receiverActivator ) where TEvent : notnull, IEvent =>
            eventReceivers.GetOrAdd( typeof( TEvent ), key => new List<Delegate>() ).Add( receiverActivator );

        /// <summary>
        /// Resolves the event receivers for the specified event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="event">The event to resolve the receivers for.</param>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of <see cref="IReceiveEvent{T}">event receivers</see>.</returns>
        public virtual IEnumerable<IReceiveEvent<TEvent>> ResolveFor<TEvent>( TEvent @event ) where TEvent : notnull, IEvent
        {
            var eventType = @event.GetType();
            var @explicit = eventReceivers.GetOrAdd( @event.GetType(), key => new List<Delegate>() ).Cast<Func<IReceiveEvent<TEvent>>>().Select( activate => activate() );
            var @dynamic = (IEnumerable<IReceiveEvent<TEvent>>) serviceProvider.GetService( typeof( IEnumerable<IReceiveEvent<TEvent>> ) ) ?? Empty<IReceiveEvent<TEvent>>();

            return @explicit.Union( @dynamic );
        }
    }
}