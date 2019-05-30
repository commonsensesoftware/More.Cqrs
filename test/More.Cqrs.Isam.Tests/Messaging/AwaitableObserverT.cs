namespace More.Domain.Messaging
{
    using System;
    using System.Threading.Tasks;

    class AwaitableObserver<T> : IObserver<T>
    {
        readonly TaskCompletionSource<T> source = new TaskCompletionSource<T>();

        public virtual void OnCompleted() => source.TrySetCanceled();

        public virtual void OnError( Exception error ) => source.TrySetException( error );

        public virtual void OnNext( T value ) => source.TrySetResult( value );

        public Task<T> Received => source.Task;
    }
}