// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the exception that is thrown when a saga configuration error occurs.
    /// </summary>
    [Serializable]
    public class SagaConfigurationException : SagaException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SagaConfigurationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">serialization information</see> used to deserialize the exception.</param>
        /// <param name="context">The <see cref="StreamingContext">context</see> in which the exception is being deserialized from.</param>
        protected SagaConfigurationException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaConfigurationException"/> class.
        /// </summary>
        public SagaConfigurationException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public SagaConfigurationException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The <see cref="Exception">error</see> that caused the current error.</param>
        public SagaConfigurationException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}