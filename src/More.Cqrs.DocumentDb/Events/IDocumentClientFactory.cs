// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Microsoft.Azure.Documents.Client;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of a DocumentDb client factory.
    /// </summary>
    [CLSCompliant( false )]
    public interface IDocumentClientFactory
    {
        /// <summary>
        /// Creates and returns a new DocumentDb client.
        /// </summary>
        /// <returns>A new, configured <see cref="DocumentClient">DocumentDb client</see>.</returns>
        DocumentClient NewClient();
    }
}