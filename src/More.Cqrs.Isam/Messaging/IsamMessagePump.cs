// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Microsoft.Database.Isam;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    sealed class IsamMessagePump : IDisposable
    {
        readonly Guid subscriptionId;
        readonly DateTimeOffset from;
        readonly IsamMessageQueueConfiguration configuration;
        readonly IIsamMessageSerializer<IMessage> messageSerializer;
        readonly IObserver<IMessageDescriptor> observer;
        readonly Throttle throttle = new Throttle();
        readonly CancellationTokenSource loop = new CancellationTokenSource();
        Task loopEnd;
        bool disposed;

        IsamMessagePump( Guid subscriptionId, DateTimeOffset from, IsamMessageQueueConfiguration configuration, IObserver<IMessageDescriptor> observer )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( observer != null );

            this.subscriptionId = subscriptionId;
            this.from = from;
            this.configuration = configuration;
            this.observer = observer;
            messageSerializer = configuration.MessageSerializer;
        }

        internal static IsamMessagePump StartNew( Guid subscriptionId, DateTimeOffset from, IsamMessageQueueConfiguration configuration, IObserver<IMessageDescriptor> observer )
        {
            Contract.Requires( configuration != null );
            Contract.Requires( observer != null );
            Contract.Ensures( Contract.Result<IsamMessagePump>() != null );

            var messagePump = new IsamMessagePump( subscriptionId, from, configuration, observer );
            messagePump.Start();
            return messagePump;
        }

#pragma warning disable CS1998
        async void Start()
#pragma warning restore CS1998
        {
            if ( loopEnd == null )
            {
                loopEnd = RunMessageLoop();
            }
        }

        async Task RunMessageLoop()
        {
            var token = loop.Token;
            var connection = configuration.CreateConnection();

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.AttachDatabase( connection.DatabaseName );

                using ( var database = session.OpenDatabase( connection.DatabaseName ) )
                {
                    CreateSubscription( database, token );

                    while ( !token.IsCancellationRequested )
                    {
                        var (dequeueOperation, timedOut) = Dequeue( database, token );

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
                                PumpMessage( database, dequeueOperation );
                            }
                        }
                    }
                }
            }

            observer.OnCompleted();
        }

        void CreateSubscription( IsamDatabase database, CancellationToken cancellationToken )
        {
            try
            {
                configuration.CreateSubscription( database, subscriptionId, from, cancellationToken );
            }
            catch ( OperationCanceledException )
            {
            }
            catch ( InvalidOperationException ) when ( cancellationToken.IsCancellationRequested )
            {
            }
            catch ( Exception error )
            {
                observer.OnError( error );
                throw;
            }
        }

        (IIsamDequeueOperation Operation, bool TimedOut) Dequeue( IsamDatabase database, CancellationToken cancellationToken )
        {
            var nextDueTime = configuration.Clock.Now;

            try
            {
                return (Operation: configuration.Dequeue( database, subscriptionId, nextDueTime ), TimedOut: false);
            }
            catch ( OperationCanceledException )
            {
            }
            catch ( InvalidOperationException ) when ( cancellationToken.IsCancellationRequested )
            {
            }
            catch ( Exception error )
            {
                observer.OnError( error );
                throw;
            }

            return (Operation: default( IIsamDequeueOperation ), TimedOut: true);
        }

        async Task ThrottleBack( IIsamDequeueOperation dequeueOperation, CancellationToken cancellationToken )
        {
            dequeueOperation.Complete();

            if ( await DelayWithoutTimingOut( throttle.Delay, cancellationToken ).ConfigureAwait( false ) )
            {
                throttle.BackOff();
            }
        }

        void PumpMessage( IsamDatabase database, IIsamDequeueOperation dequeueOperation )
        {
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
                configuration.Enqueue( database, item );
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

            try
            {
                loopEnd.GetAwaiter().GetResult();
            }
            finally
            {
                loopEnd.Dispose();
            }
        }

        sealed class Throttle
        {
            readonly Stopwatch timer = Stopwatch.StartNew();

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