// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using More.Domain.Options;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IMessageDescriptor ) )]
    abstract class IMessageDescriptorContract : IMessageDescriptor
    {
        object IMessageDescriptor.AggregateId
        {
            get
            {
                Contract.Ensures( Contract.Result<object>() != null );
                return null;
            }
        }

        string IMessageDescriptor.MessageId
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );
                return null;
            }
        }

        string IMessageDescriptor.MessageType
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );
                return null;
            }
        }

        IMessage IMessageDescriptor.Message
        {
            get
            {
                Contract.Ensures( Contract.Result<IMessage>() != null );
                return null;
            }
        }

        IOptions IMessageDescriptor.Options
        {
            get
            {
                Contract.Ensures( Contract.Result<IOptions>() != null );
                return null;
            }
        }
    }
}