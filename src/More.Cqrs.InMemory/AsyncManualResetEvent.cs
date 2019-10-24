// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    // REF: https://github.com/StephenCleary/AsyncEx/blob/master/Source/Nito.AsyncEx%20(NET45%2C%20Win8%2C%20WP8%2C%20WPA81)/AsyncManualResetEvent.cs
    sealed class AsyncManualResetEvent
    {
        readonly object syncRoot = new object();
        TaskCompletionSource<object?> tcs = new TaskCompletionSource<object?>();

        internal AsyncManualResetEvent() => tcs.SetResult( null );

        internal Task WaitAsync()
        {
            lock ( syncRoot )
            {
                return tcs.Task;
            }
        }

        internal void Wait() => WaitAsync().Wait();

        internal void Wait( CancellationToken cancellationToken )
        {
            var task = WaitAsync();

            if ( !task.IsCompleted )
            {
                task.Wait( cancellationToken );
            }
        }

        internal void Set()
        {
            lock ( syncRoot )
            {
                Run( () => tcs.TrySetResult( null ) );
                tcs.Task.Wait();
            }
        }

        internal void Set( Exception error )
        {
            lock ( syncRoot )
            {
                Run( () => tcs.TrySetException( error ) );
                tcs.Task.Wait();
            }
        }

        internal void Reset()
        {
            lock ( syncRoot )
            {
                if ( tcs.Task.IsCompleted )
                {
                    tcs = new TaskCompletionSource<object?>();
                }
            }
        }
    }
}