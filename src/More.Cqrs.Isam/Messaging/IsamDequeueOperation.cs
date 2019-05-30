// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Microsoft.Database.Isam;
    using System.Diagnostics.Contracts;

    sealed class IsamDequeueOperation : IIsamDequeueOperation
    {
        readonly IsamTransaction transaction;
        bool completed;
        bool disposed;

        internal IsamDequeueOperation( IsamMessageQueueItem item )
        {
            Contract.Requires( item != null );
            Contract.Requires( transaction != null );

            Item = item;
            transaction = item.Transaction;
        }

        internal static IIsamDequeueOperation Empty { get; } = new EmptyIsamDequeueOperation();

        public IsamMessageQueueItem Item { get; }

        public void Complete()
        {
            if ( completed )
            {
                return;
            }

            completed = true;
            transaction.Commit();
        }

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            Item.Dispose();

            if ( !completed )
            {
                transaction.Rollback();
            }

            transaction.Dispose();
        }

        sealed class EmptyIsamDequeueOperation : IIsamDequeueOperation
        {
            internal EmptyIsamDequeueOperation() { }

            public IsamMessageQueueItem Item => null;

            public void Complete() { }

            public void Dispose() { }
        }
    }
}