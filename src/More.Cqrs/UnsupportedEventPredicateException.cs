﻿// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the exception that is thrown when an unsupported <see cref="IEventPredicate{TKey}">event predicate</see> is encountered.
    /// </summary>
    [Serializable]
    public class UnsupportedEventPredicateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedEventPredicateException"/> class.
        /// </summary>
        /// <param name="info">The exception <see cref="SerializationInfo">serialization information</see>.</param>
        /// <param name="context">The serialization <see cref="StreamingContext">streaming context</see>.</param>
        protected UnsupportedEventPredicateException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedEventPredicateException"/> class.
        /// </summary>
        public UnsupportedEventPredicateException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedEventPredicateException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public UnsupportedEventPredicateException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedEventPredicateException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public UnsupportedEventPredicateException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}