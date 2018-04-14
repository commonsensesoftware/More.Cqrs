namespace More.Domain.Messaging
{
    using System;

    class FailOnceObserver<T> : AwaitableObserver<T>
    {
        bool failed;
        Exception onNextError;

        public FailOnceObserver( Exception error ) => onNextError = error;

        public override void OnError( Exception error ) => Error = error;

        public override void OnNext( T value )
        {
            MessagesReceived++;

            if ( failed )
            {
                base.OnNext( value );
            }
            else
            {
                failed = true;
                throw onNextError;
            }
        }

        public int MessagesReceived { get; private set; }

        public Exception Error { get; private set; }
    }
}