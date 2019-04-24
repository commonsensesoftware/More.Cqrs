// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.TaskCreationOptions;

    sealed class MessagePump : IDisposable
    {
        readonly CancellationTokenSource source = new CancellationTokenSource();
        readonly BlockingCollection<IMessageDescriptor> messages;
        readonly IObserver<IMessageDescriptor> observer;
        bool disposed;

        MessagePump( BlockingCollection<IMessageDescriptor> messages, IObserver<IMessageDescriptor> observer )
        {
            Contract.Requires( messages != null );
            Contract.Requires( observer != null );

            this.messages = messages;
            this.observer = observer;
        }

        internal static MessagePump StartNew( BlockingCollection<IMessageDescriptor> messages, IObserver<IMessageDescriptor> observer )
        {
            Contract.Requires( messages != null );
            Contract.Requires( observer != null );
            Contract.Ensures( Contract.Result<MessagePump>() != null );

            var messagePump = new MessagePump( messages, observer );
            messagePump.Start();
            return messagePump;
        }

        void Start() => Task.Factory.StartNew( Run, CancellationToken.None, LongRunning | HideScheduler, TaskScheduler.Default );

        void Run()
        {
            var token = source.Token;

            try
            {
                foreach ( var message in messages.GetConsumingEnumerable( token ) )
                {
                    observer.OnNext( message );
                }
            }
            catch ( OperationCanceledException )
            {
            }
            finally
            {
                observer.OnCompleted();
            }
        }

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            source.Cancel();
            source.Dispose();
            messages.Dispose();
        }
    }
}