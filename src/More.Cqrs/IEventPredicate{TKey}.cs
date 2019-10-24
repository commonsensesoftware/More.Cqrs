// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1040 // Avoid empty interfaces

namespace More.Domain
{
    using More.Domain.Events;

    /// <summary>
    /// Defines the behavior of an <see cref="IEvent">event</see> predicate.
    /// </summary>
    /// <typeparam name="TKey">The type of event key.</typeparam>
    public interface IEventPredicate<TKey> where TKey : notnull
    {
    }
}