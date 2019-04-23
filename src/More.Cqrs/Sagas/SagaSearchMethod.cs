// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;

    /// <summary>
    /// Represents a saga search method.
    /// </summary>
    public class SagaSearchMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SagaSearchMethod"/> class.
        /// </summary>
        /// <param name="type">The type used to search for a saga.</param>
        /// <param name="messageType">The message type the search method is used for.</param>
        public SagaSearchMethod( Type type, Type messageType )
        {
            Arg.NotNull( type, nameof( type ) );
            Arg.NotNull( messageType, nameof( messageType ) );

            Type = type;
            MessageType = messageType;
        }

        /// <summary>
        /// Gets the search method type.
        /// </summary>
        /// <value>The type of search method.</value>
        /// <remarks>The type is a class that implements <see cref="ISearchForSaga"/>.</remarks>
#pragma warning disable CA1721 // Property names should not match get methods
        public Type Type { get; }
#pragma warning restore CA1721 // Property names should not match get methods

        /// <summary>
        /// Gets the message type the search method is used for.
        /// </summary>
        /// <value>The message type the search method applies to.</value>
        public Type MessageType { get; }
    }
}