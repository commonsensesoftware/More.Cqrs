// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Options
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of message options.
    /// </summary>
    [ContractClass( typeof( IOptionsContract ) )]
    public interface IOptions
    {
        /// <summary>
        /// Gets a configured option.
        /// </summary>
        /// <typeparam name="T">The type of option to retrieve.</typeparam>
        /// <returns>The requested message option.</returns>
        T Get<T>() where T : class;

        /// <summary>
        /// Attempts to retrieve a configured message option.
        /// </summary>
        /// <typeparam name="T">The type of option to retrieve.</typeparam>
        /// <param name="option">The requested message option, if present.</param>
        /// <returns>True if the requested option exists; otherwise, false.</returns>
        bool TryGet<T>( out T option ) where T : class;

        /// <summary>
        /// Gets all of the configured options.
        /// </summary>
        /// <typeparam name="T">The type of configured options to retrieve.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of the configured options.</returns>
        IEnumerable<T> All<T>() where T : class;
    }
}