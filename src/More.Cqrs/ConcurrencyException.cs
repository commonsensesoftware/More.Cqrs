// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the exception that is thrown when a concurrency check fails.
    /// </summary>
    [Serializable]
    public class ConcurrencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="info">The exception <see cref="SerializationInfo">serialization information</see>.</param>
        /// <param name="context">The serialization <see cref="StreamingContext">streaming context</see>.</param>
        protected ConcurrencyException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        public ConcurrencyException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ConcurrencyException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public ConcurrencyException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}