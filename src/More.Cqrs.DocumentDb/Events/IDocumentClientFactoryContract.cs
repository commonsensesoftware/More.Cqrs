// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Microsoft.Azure.Documents.Client;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IDocumentClientFactory ) )]
    abstract class IDocumentClientFactoryContract : IDocumentClientFactory
    {
        DocumentClient IDocumentClientFactory.NewClient()
        {
            Contract.Ensures( Contract.Result<DocumentClient>() != null );
            return null;
        }
    }
}