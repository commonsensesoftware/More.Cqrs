// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Events
{
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Security;

#pragma warning disable CA1001 // SecureString should not be disposed

    /// <summary>
    /// Represents a factory used to configure and create <see cref="DocumentClient">DocumentDb clients</see>.
    /// </summary>
    public class DocumentClientFactory : IDocumentClientFactory
    {
        Uri serviceEndpoint;
        SecureString authorizationKey;
        List<Permission> permissionFeed = new List<Permission>();
        ConnectionPolicy connectionPolicy = null;
        ConsistencyLevel? desiredConsistencyLevel;

        /// <summary>
        /// Uses the configured service endpoint Uniform Resource Locator (URL).
        /// </summary>
        /// <param name="value">The service endpoint <see cref="Uri">URL</see>.</param>
        /// <returns>The original <see cref="DocumentClientFactory"/> instance.</returns>
        public virtual DocumentClientFactory UseServiceEndpoint( Uri value )
        {
            serviceEndpoint = value;
            return this;
        }

        /// <summary>
        /// Uses the configured authorization key.
        /// </summary>
        /// <param name="value">The authorization key.</param>
        /// <returns>The original <see cref="DocumentClientFactory"/> instance.</returns>
        public virtual DocumentClientFactory UseAuthorizationKey( string value )
        {
            authorizationKey?.Dispose();
            authorizationKey = new SecureString();

            foreach ( var @char in value )
            {
                authorizationKey.AppendChar( @char );
            }

            authorizationKey.MakeReadOnly();

            return this;
        }

        /// <summary>
        /// Uses the configured authorization key.
        /// </summary>
        /// <param name="value">The authorization key stored in a <see cref="SecureString">secure string</see>.</param>
        /// <returns>The original <see cref="DocumentClientFactory"/> instance.</returns>
        public virtual DocumentClientFactory UseAuthorizationKey( SecureString value )
        {
            authorizationKey?.Dispose();
            authorizationKey = value;
            return this;
        }

        /// <summary>
        /// Indicates that a client should have the specified permissions.
        /// </summary>
        /// <param name="values">The <see cref="Permission">permissions</see> a client should have.</param>
        /// <returns>The original <see cref="DocumentClientFactory"/> instance.</returns>
        [CLSCompliant( false )]
        public virtual DocumentClientFactory HasPermissions( params Permission[] values )
        {
            permissionFeed.AddRange( values );
            return this;
        }

        /// <summary>
        /// Indicates that a client should have the specified consistency level.
        /// </summary>
        /// <param name="value">The desired <see cref="ConsistencyLevel">consistency level</see>.</param>
        /// <returns>The original <see cref="DocumentClientFactory"/> instance.</returns>
        [CLSCompliant( false )]
        public virtual DocumentClientFactory HasConsistencyLevel( ConsistencyLevel value )
        {
            desiredConsistencyLevel = value;
            return this;
        }

        /// <summary>
        /// Uses the configured connection policy.
        /// </summary>
        /// <param name="value">The configured <see cref="ConnectionPolicy">connection policy</see>.</param>
        /// <returns>The original <see cref="DocumentClientFactory"/> instance.</returns>
        [CLSCompliant( false )]
        public virtual DocumentClientFactory UseConnectionPolicy( ConnectionPolicy value )
        {
            connectionPolicy = value;
            return this;
        }

        /// <summary>
        /// Creates and returns a new DocumentDb client.
        /// </summary>
        /// <returns>A new, configured <see cref="DocumentClient">DocumentDb client</see>.</returns>
        [CLSCompliant( false )]
        public virtual DocumentClient NewClient()
        {
            if ( serviceEndpoint == null )
            {
                throw new InvalidOperationException( SR.MissingDocumentDbServiceEndpoint );
            }

            if ( permissionFeed.Count > 0 )
            {
                return new DocumentClient( serviceEndpoint, permissionFeed, connectionPolicy, desiredConsistencyLevel );
            }

            if ( authorizationKey == null )
            {
                throw new InvalidOperationException( SR.MissingDocumentDbAuth );
            }

            return new DocumentClient( serviceEndpoint, authorizationKey, connectionPolicy, desiredConsistencyLevel );
        }
    }
}