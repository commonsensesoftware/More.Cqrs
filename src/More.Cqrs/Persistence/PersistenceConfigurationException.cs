// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the exception that is thrown when there is a persistence configuration error.
    /// </summary>
    [Serializable]
    public class PersistenceConfigurationException : PersistenceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceConfigurationException"/> class.
        /// </summary>
        /// <param name="info">The exception <see cref="SerializationInfo">serialization information</see>.</param>
        /// <param name="context">The serialization <see cref="StreamingContext">streaming context</see>.</param>
        protected PersistenceConfigurationException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceConfigurationException"/> class.
        /// </summary>
        public PersistenceConfigurationException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PersistenceConfigurationException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public PersistenceConfigurationException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}