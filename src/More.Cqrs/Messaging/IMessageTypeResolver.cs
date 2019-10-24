// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of an object that can resolve message types from their names.
    /// </summary>
    public interface IMessageTypeResolver
    {
        /// <summary>
        /// Resolves a message type from the given type name.
        /// </summary>
        /// <param name="typeName">The message type name to resolve.</param>
        /// <param name="revision">The revision of the message type.</param>
        /// <returns>The resolved message <see cref="Type">type</see>.</returns>
        Type ResolveType( string typeName, int revision );
    }
}