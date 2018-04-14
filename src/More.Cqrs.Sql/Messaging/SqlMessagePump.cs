// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    sealed class SqlMessagePump : IDisposable
    {
        readonly Guid subscriptionId;
        readonly DateTimeOffset from;
        readonly SqlMessageQueueConfiguration configuration;
        readonly ISqlMessageSerializer<IMessage> messageSerializer;
        readonly IObserver<IMessageDescriptor> observer;
        readonly Throttle throttle = new Throttle();
        readonly CancellationTokenSource loop = new CancellationTokenSource();
        bool disposed;

        SqlMessagePump( Guid subscriptionId, DateTimeOffset from, SqlMessageQueueConfiguration configuration, IObserver<IMessageDescriptor> observer )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( observer != null );

            this.subscriptionId = subscriptionId;
            this.from = from;
            this.configuration = configuration;
            this.observer = observer;
            messageSerializer = configuration.MessageSerializer;
        }

        internal static SqlMessagePump StartNew( Guid subscriptionId, DateTimeOffset from, SqlMessageQueueConfiguration configuration, IObserver<IMessageDescriptor> observer )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( observer != null );
            Contract.Ensures( Contract.Result<SqlMessagePump>() != null );

            var messagePump = new SqlMessagePump( subscriptionId, from, configuration, observer );
            messagePump.Start();
            return messagePump;
        }

        async void Start()
        {
            var token = loop.Token;

            using ( var connection = configuration.CreateConnection() )
            {
                await CreateSubscription( connection, token ).ConfigureAwait( false );

                while ( !token.IsCancellationRequested )
                {
                    var (dequeueOperation, timedOut) = await Dequeue( connection, token ).ConfigureAwait( false );

                    using ( dequeueOperation )
                    {
                        if ( timedOut )
                        {
                            continue;
                        }

                        if ( dequeueOperation.Item == null )
                        {
                            await ThrottleBack( dequeueOperation, token ).ConfigureAwait( false );
                        }
                        else
                        {
                            await PumpMessage( connection, dequeueOperation ).ConfigureAwait( false );
                        }
                    }
                }
            }

            observer.OnCompleted();
        }

        async Task CreateSubscription( DbConnection connection, CancellationToken cancellationToken )
        {
            try
            {
                await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
                await configuration.CreateSubscription( connection, subscriptionId, from, cancellationToken ).ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
            }
            catch ( InvalidOperationException ) when ( cancellationToken.IsCancellationRequested )
            {
            }
        }

        async Task<(ISqlDequeueOperation Operation, bool TimedOut)> Dequeue( DbConnection connection, CancellationToken cancellationToken )
        {
            Contract.Requires( connection != null );
            Contract.Ensures( Contract.Result<IMessageDescriptor>() != null );

            var nextDueTime = configuration.Clock.Now;

            try
            {
                return (Operation: await configuration.Dequeue( connection, subscriptionId, nextDueTime, cancellationToken ).ConfigureAwait( false ), TimedOut: false);
            }
            catch ( OperationCanceledException )
            {
            }
            catch ( InvalidOperationException ) when ( cancellationToken.IsCancellationRequested )
            {
            }

            return (Operation: default( ISqlDequeueOperation ), TimedOut: true);
        }

        async Task ThrottleBack( ISqlDequeueOperation dequeueOperation, CancellationToken cancellationToken )
        {
            Contract.Requires( dequeueOperation != null );

            dequeueOperation.Complete();

            if ( await DelayWithoutTimingOut( throttle.Delay, cancellationToken ).ConfigureAwait( false ) )
            {
                throttle.BackOff();
            }
        }

        async Task PumpMessage( DbConnection connection, ISqlDequeueOperation dequeueOperation )
        {
            Contract.Requires( connection != null );
            Contract.Requires( dequeueOperation != null );

            var item = dequeueOperation.Item;
            var (messageType, revision, stream) = item;

            try
            {
                var message = messageSerializer.Deserialize( messageType, revision, stream ).GetDescriptor();
                observer.OnNext( message );
                dequeueOperation.Complete();
            }
            catch ( Exception error )
            {
                item.EnqueueTime = configuration.Clock.Now.UtcDateTime;
                item.DequeueAttempts++;
                stream.Position = 0L;
                await configuration.Enqueue( connection, item, CancellationToken.None ).ConfigureAwait( false );
                dequeueOperation.Complete();
                observer.OnError( error );
            }
            finally
            {
                throttle.Reset();
            }
        }

        static async Task<bool> DelayWithoutTimingOut( int delayInMilliseconds, CancellationToken cancellationToken )
        {
            try
            {
                await Delay( delayInMilliseconds, cancellationToken ).ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            loop.Cancel();
            loop.Dispose();
        }

        sealed class Throttle
        {
            Stopwatch timer = Stopwatch.StartNew();

            internal void BackOff()
            {
                if ( timer.ElapsedMilliseconds < 1000L )
                {
                }
                else if ( timer.ElapsedMilliseconds < 2000L )
                {
                    Delay = 250;
                }
                else if ( timer.ElapsedMilliseconds < 3000L )
                {
                    Delay = 500;
                }
                else
                {
                    Delay = 1000;
                }
            }

            internal void Reset()
            {
                Delay = 100;
                timer.Reset();
            }

            internal int Delay { get; private set; }
        }
    }
}