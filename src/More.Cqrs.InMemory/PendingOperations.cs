// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a set of pending operations.
    /// </summary>
    [DebuggerDisplay( "Count = {count}" )]
    public sealed class PendingOperations : IDisposable
    {
        readonly AsyncManualResetEvent none = new AsyncManualResetEvent();
        bool disposed;
        volatile int count;

        /// <summary>
        /// Releases the managed resources used by the <see cref="PendingOperations"/> class.
        /// </summary>
        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            none.Set();
        }

        /// <summary>
        /// Increments the number of pending operations.
        /// </summary>
        public void Increment()
        {
            none.Reset();
            count++;
        }

        /// <summary>
        /// Increments the number of pending operations by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to increment the pending message count by.</param>
        public void IncrementBy( int amount )
        {
            if ( amount > 0 )
            {
                none.Reset();
                count += amount;
            }
        }

        /// <summary>
        /// Decrements the number of pending operations.
        /// </summary>
        public void Decrement()
        {
            if ( --count <= 0 )
            {
                count = 0;
                none.Set();
            }
        }

        /// <summary>
        /// Asynchronously waits until there no pending operations.
        /// </summary>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public Task None() => none.WaitAsync();

        /// <summary>
        /// Observes the specified error on the handle awaiting completion.
        /// </summary>
        /// <param name="error">The <see cref="Exception">error</see> that occurred.</param>
        public void Observe( Exception error ) => none.Set( error );
    }
}