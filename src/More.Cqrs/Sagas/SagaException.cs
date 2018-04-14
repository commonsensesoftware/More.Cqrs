// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the base exception for saga-based errors.
    /// </summary>
    [Serializable]
    public class SagaException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SagaException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">serialization information</see> used to deserialize the exception.</param>
        /// <param name="context">The <see cref="StreamingContext">context</see> in which the exception is being deserialized from.</param>
        protected SagaException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaException"/> class.
        /// </summary>
        public SagaException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public SagaException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The <see cref="Exception">error</see> that caused the current error.</param>
        public SagaException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}