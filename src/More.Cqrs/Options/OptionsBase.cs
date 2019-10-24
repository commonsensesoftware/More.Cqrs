// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Options
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents the base implementation for message options.
    /// </summary>
    public abstract class OptionsBase : IOptions
    {
        readonly KeyedCollection<Type, object> items = new KeyedCollection<Type, object>( o => o.GetType() );

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsBase"/> class.
        /// </summary>
        protected OptionsBase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsBase"/> class.
        /// </summary>
        /// <param name="options">The initial <see cref="IEnumerable{T}">sequence</see> of options to add.</param>
        /// <remarks>This constructor is useful for object deserialization.</remarks>
        protected OptionsBase( IEnumerable<object> options )
        {
            foreach ( var option in options )
            {
                items.Add( option );
            }
        }

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <typeparam name="T">The type of option to add.</typeparam>
        /// <param name="option">The option to add.</param>
        public void Add<T>( T option ) where T : class => items.Add( option );

        /// <summary>
        /// Attempts to add a new option.
        /// </summary>
        /// <typeparam name="T">The type of option to add.</typeparam>
        /// <param name="option">The option to add.</param>
        /// <returns>True if the option was added; otherwise, false.</returns>
        /// <remarks>This method can be used to handle a scenarios where an option
        /// of the same type might be added more than once.</remarks>
        public bool TryAdd<T>( T option ) where T : class
        {
            if ( items.Contains( option.GetType() ) )
            {
                return false;
            }

            items.Add( option );
            return true;
        }

        /// <summary>
        /// Gets a configured option.
        /// </summary>
        /// <typeparam name="T">The type of option to retrieve.</typeparam>
        /// <returns>The requested message option.</returns>
        public T Get<T>() where T : notnull => (T) items[typeof( T )];

        /// <summary>
        /// Attempts to retrieve a configured message option.
        /// </summary>
        /// <typeparam name="T">The type of option to retrieve.</typeparam>
        /// <param name="option">The requested message option, if present.</param>
        /// <returns>True if the requested option exists; otherwise, false.</returns>
        public bool TryGet<T>([NotNullWhen(true)] out T option ) where T : class
        {
            if ( items.TryGetValue( typeof( T ), out object value ) )
            {
                option = (T) value;
                return true;
            }

            option = default!;
            return false;
        }

        /// <summary>
        /// Gets all of the configured options.
        /// </summary>
        /// <typeparam name="T">The type of configured options to retrieve.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of the configured options.</returns>
        public IEnumerable<T> All<T>() where T : notnull => items.OfType<T>();
    }
}