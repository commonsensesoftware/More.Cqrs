// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Collections.Generic;

    sealed class SagaMessageComparer : IEqualityComparer<SagaMessage>
    {
        SagaMessageComparer() { }

        internal static IEqualityComparer<SagaMessage> Instance { get; } = new SagaMessageComparer();

        public bool Equals( SagaMessage x, SagaMessage y )
        {
            if ( x == null )
            {
                return y == null;
            }

            if ( y == null )
            {
                return false;
            }

            return x.MessageType == y.MessageType;
        }

        public int GetHashCode( SagaMessage obj ) => obj?.MessageType.GetHashCode() ?? 0;
    }
}