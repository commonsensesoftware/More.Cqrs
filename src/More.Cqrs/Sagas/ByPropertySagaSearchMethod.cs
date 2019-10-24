// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay( "Message = {MessageType.Name}, Property = {PropertyName}" )]
    sealed class ByPropertySagaSearchMethod : SagaSearchMethod
    {
        internal ByPropertySagaSearchMethod( Type type, Type messageType, string propertyName, Func<object, object> readMessageProperty )
            : base( type, messageType )
        {
            PropertyName = propertyName;
            ReadMessageProperty = readMessageProperty;
        }

        internal string PropertyName { get; }

        internal Func<object, object> ReadMessageProperty { get; }
    }
}