// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Messaging
{
    using Options;
    using System.Diagnostics;

    /// <summary>
    /// Represents the base implementation for a message descriptor.
    /// </summary>
    /// <typeparam name="TKey">The type of aggregate identifier.</typeparam>
    [DebuggerDisplay( "{DebuggerDisplay}" )]
    public abstract class MessageDescriptor<TKey> : IMessageDescriptor
    {
        readonly IMessage message;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDescriptor{TKey}"/> class.
        /// </summary>
        /// <param name="aggregateId">The associated aggregate identifier.</param>
        /// <param name="message">The described message.</param>
        /// <param name="options">The associated <see cref="IOptions">options</see>.</param>
        protected MessageDescriptor( TKey aggregateId, IMessage message, IOptions options )
        {
            Arg.NotNull( message, nameof( message ) );
            Arg.NotNull( options, nameof( options ) );

            AggregateId = aggregateId;
            MessageType = message.GetType().GetAssemblyQualifiedName();
            this.message = message;
            Options = options;
        }

        string DebuggerDisplay => $"{message?.GetType().Name ?? "null"} (Id: {MessageId})";

        /// <summary>
        /// Gets the unique message identifier.
        /// </summary>
        /// <value>The unique message identifier.</value>
        public abstract string MessageId { get; }

        /// <summary>
        /// Gets the qualified type name of the described message.
        /// </summary>
        /// <value>The qualified type name of the described message.</value>
        /// <remarks>The default implementation returns the type and assembly name without any version information.</remarks>
        public virtual string MessageType { get; }

        /// <summary>
        /// Gets the associated aggregate identifier.
        /// </summary>
        /// <value>The associated aggregate identifier.</value>
        public TKey AggregateId { get; }

#pragma warning disable CA1033 // Interface methods should be callable by child types
        object IMessageDescriptor.AggregateId => AggregateId;

        IMessage IMessageDescriptor.Message => message;
#pragma warning restore CA1033 // Interface methods should be callable by child types

        /// <summary>
        /// Gets the options associated with the descriptor.
        /// </summary>
        /// <value>The associated <see cref="IOptions">options</see>.</value>
        public IOptions Options { get; }
    }
}