// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;

    /// <summary>
    /// Defines the behavior of an ISAM dequeue operation.
    /// </summary>
    public interface IIsamDequeueOperation : IDisposable
    {
        /// <summary>
        /// Gets the dequeued item.
        /// </summary>
        /// <value>The dequeued <see cref="IsamMessageQueueItem">item</see>.</value>
        IsamMessageQueueItem Item { get; }

        /// <summary>
        /// Completes the operation.
        /// </summary>
        void Complete();
    }
}