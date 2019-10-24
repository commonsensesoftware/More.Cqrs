// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable CA1716 // Identifiers should not match keywords

namespace More.Domain.Options
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the behavior of message options.
    /// </summary>
    public interface IOptions
    {
        /// <summary>
        /// Gets a configured option.
        /// </summary>
        /// <typeparam name="T">The type of option to retrieve.</typeparam>
        /// <returns>The requested message option.</returns>
        T Get<T>() where T : notnull;

        /// <summary>
        /// Attempts to retrieve a configured message option.
        /// </summary>
        /// <typeparam name="T">The type of option to retrieve.</typeparam>
        /// <param name="option">The requested message option, if present.</param>
        /// <returns>True if the requested option exists; otherwise, false.</returns>
        bool TryGet<T>( [MaybeNullWhen( false )] out T option ) where T : class;

        /// <summary>
        /// Gets all of the configured options.
        /// </summary>
        /// <typeparam name="T">The type of configured options to retrieve.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of the configured options.</returns>
        IEnumerable<T> All<T>() where T : notnull;
    }
}