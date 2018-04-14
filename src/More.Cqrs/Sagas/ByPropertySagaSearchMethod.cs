// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    [DebuggerDisplay( "Message = {MessageType.Name}, Property = {PropertyName}" )]
    sealed class ByPropertySagaSearchMethod : SagaSearchMethod
    {
        internal ByPropertySagaSearchMethod( Type type, Type messageType, string propertyName, Func<object, object> readMessageProperty )
            : base( type, messageType )
        {
            Contract.Requires( type != null );
            Contract.Requires( messageType != null );
            Contract.Requires( !string.IsNullOrEmpty( propertyName ) );
            Contract.Requires( readMessageProperty != null );

            PropertyName = propertyName;
            ReadMessageProperty = readMessageProperty;
        }

        internal string PropertyName { get; }

        internal Func<object, object> ReadMessageProperty { get; }
    }
}