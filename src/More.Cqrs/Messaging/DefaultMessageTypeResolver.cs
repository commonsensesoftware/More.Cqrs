// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using System;

    /// <summary>
    /// Represents a deault message type resolver.
    /// </summary>
    public sealed class DefaultMessageTypeResolver : IMessageTypeResolver
    {
        /// <summary>
        /// Resolves a message type from the given type name.
        /// </summary>
        /// <param name="typeName">The message type name to resolve.</param>
        /// <param name="revision">The revision of the message type.</param>
        /// <returns>The resolved message <see cref="Type">type</see>.</returns>
        /// <remarks>Types are resolved using <see cref="Type.GetType(string, bool, bool)"/>.</remarks>
        public Type ResolveType( string typeName, int revision )
        {
            Arg.NotNullOrEmpty( typeName, nameof( typeName ) );

            return Type.GetType( typeName, throwOnError: true, ignoreCase: false );
        }
    }
}