// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;

    [ContractClassFor( typeof( ISqlMessageSerializer<> ) )]
    abstract class ISqlMessageSerializerContract<TMessage> : ISqlMessageSerializer<TMessage> where TMessage : class
    {
        TMessage ISqlMessageSerializer<TMessage>.Deserialize( string messageType, int revision, Stream message )
        {
            Contract.Requires<ArgumentNullException>( !string.IsNullOrEmpty( messageType ), nameof( messageType ) );
            Contract.Requires<ArgumentNullException>( message != null, nameof( message ) );
            Contract.Ensures( Contract.Result<TMessage>() != null );
            return null;
        }

        Stream ISqlMessageSerializer<TMessage>.Serialize( TMessage message )
        {
            Contract.Requires<ArgumentNullException>( message != null, nameof( message ) );
            Contract.Ensures( Contract.Result<Stream>() != null );
            return null;
        }
    }
}