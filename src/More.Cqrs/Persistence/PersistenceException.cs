// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the exception that is thrown when a persistence error occurs.
    /// </summary>
    [Serializable]
    public class PersistenceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceException"/> class.
        /// </summary>
        /// <param name="info">The exception <see cref="SerializationInfo">serialization information</see>.</param>
        /// <param name="context">The serialization <see cref="StreamingContext">streaming context</see>.</param>
        protected PersistenceException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceException"/> class.
        /// </summary>
        public PersistenceException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PersistenceException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public PersistenceException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}