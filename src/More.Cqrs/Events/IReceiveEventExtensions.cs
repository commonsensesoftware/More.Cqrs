// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using System;

    /// <summary>
    /// Provides extension methods for the <see cref="IReceiveEvent{TEvent}"/> interface.
    /// </summary>
    public static class IReceiveEventExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the event receiver is a saga.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventReceiver">The <see cref="IReceiveEvent{T}">event receiver</see> to evaluate.</param>
        /// <returns>True if the <paramref name="eventReceiver">event receiver</paramref> is a saga; otherwise, false.</returns>
        public static bool IsSaga<TEvent>( this IReceiveEvent<TEvent> eventReceiver ) where TEvent : IEvent
        {
            Arg.NotNull( eventReceiver, nameof( eventReceiver ) );
            return eventReceiver.GetType().IsSaga();
        }
    }
}