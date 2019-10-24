// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;

    /// <summary>
    /// Defines the behavior of a SQL dequeue operation.
    /// </summary>
    public interface ISqlDequeueOperation : IDisposable
    {
        /// <summary>
        /// Gets the dequeued item.
        /// </summary>
        /// <value>The dequeued <see cref="SqlMessageQueueItem">item</see>.</value>
        SqlMessageQueueItem? Item { get; }

        /// <summary>
        /// Completes the operation.
        /// </summary>
        void Complete();
    }
}