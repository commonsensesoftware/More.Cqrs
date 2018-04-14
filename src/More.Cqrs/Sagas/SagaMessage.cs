// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;

    /// <summary>
    /// Represents a saga-related message.
    /// </summary>
    public class SagaMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SagaMessage"/> class.
        /// </summary>
        /// <param name="messageType">The type of message associated with a saga.</param>
        /// <param name="startsSaga">Indicates whether the message starts a saga.</param>
        public SagaMessage( Type messageType, bool startsSaga )
        {
            Arg.NotNull( messageType, nameof( messageType ) );

            MessageType = messageType;
            StartsSaga = startsSaga;
        }

        /// <summary>
        /// Gets the associated message type.
        /// </summary>
        /// <value>The associated message type.</value>
        public Type MessageType { get; }

        /// <summary>
        /// Gets a value indicating whether the message starts a saga.
        /// </summary>
        /// <value>True if the message starts a saga; otherwise, false.</value>
        public bool StartsSaga { get; }
    }
}