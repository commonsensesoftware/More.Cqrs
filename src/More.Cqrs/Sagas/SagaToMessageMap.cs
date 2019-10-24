// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    sealed class SagaToMessageMap
    {
        internal SagaToMessageMap(
            Type messageType,
            PropertyInfo messageProperty,
            PropertyInfo sagaDataProperty,
            Func<object, object> readMessageProperty )
        {
            MessageType = messageType;
            MessageProperty = messageProperty;
            SagaDataProperty = sagaDataProperty;
            ReadMessageProperty = readMessageProperty;
        }

        internal Type MessageType { get; }

        internal PropertyInfo MessageProperty { get; }

        internal PropertyInfo SagaDataProperty { get; }

        internal Func<object, object> ReadMessageProperty { get; }
    }
}