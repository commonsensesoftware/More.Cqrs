// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Options;
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IMessage ) )]
    abstract class IMessageContract : IMessage
    {
        int IMessage.Revision => default( int );

        string IMessage.CorrelationId => default( string );

        IMessageDescriptor IMessage.GetDescriptor( IOptions options )
        {
            Contract.Requires<ArgumentNullException>( options != null, nameof( options ) );
            Contract.Ensures( Contract.Result<IMessageDescriptor>() != null );
            return null;
        }
    }
}