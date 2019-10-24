// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System.Data.Common;

    sealed class SqlDequeueOperation : ISqlDequeueOperation
    {
        readonly DbTransaction transaction;
        bool completed;
        bool disposed;

        internal SqlDequeueOperation( SqlMessageQueueItem item, DbTransaction transaction )
        {
            Item = item;
            this.transaction = transaction;
        }

        internal static ISqlDequeueOperation Empty { get; } = new EmptySqlDequeueOperation();

        public SqlMessageQueueItem? Item { get; }

        public void Complete()
        {
            transaction.Commit();
            completed = true;
        }

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            Item?.Dispose();

            if ( !completed )
            {
                transaction.Rollback();
            }

            transaction.Dispose();
        }

        sealed class EmptySqlDequeueOperation : ISqlDequeueOperation
        {
            internal EmptySqlDequeueOperation() { }

            public SqlMessageQueueItem? Item => null;

            public void Complete() { }

            public void Dispose() { }
        }
    }
}