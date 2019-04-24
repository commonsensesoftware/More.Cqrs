// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Options;
    using System.Diagnostics;

    /// <summary>
    /// Represents the base implementation for a command.
    /// </summary>
    /// <typeparam name="TKey">The type of key for the command.</typeparam>
    [DebuggerDisplay( "{GetType().Name}, Version = {ExpectedVersion}, AggregateId = {AggregateId}" )]
    public abstract class Command<TKey> : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command{TKey}"/> class.
        /// </summary>
        protected Command() => CorrelationId = Correlation.CurrentId;

        /// <summary>
        /// Gets or sets the associated aggregate identifier.
        /// </summary>
        /// <value>The associated aggregate identifier.</value>
        public TKey AggregateId { get; set; }

        /// <summary>
        /// Gets or sets the associated aggregate version.
        /// </summary>
        /// <value>The version of the aggregate when the command was created.</value>
        public int ExpectedVersion { get; set; } = Events.ExpectedVersion.Initial;

        /// <summary>
        /// Gets or sets the revision of the command.
        /// </summary>
        /// <value>The command revision.</value>
        /// <remarks>The command revision can be used to handle different versions of a command over time.</remarks>
        public int Revision { get; set; } = 1;

        /// <summary>
        /// Gets or sets the associated correlation identifier.
        /// </summary>
        /// <value>The associated correlation identifier.</value>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Creates and returns a descriptor for the current command.
        /// </summary>
        /// <param name="options">The <see cref="IOptions">options</see> associated with the command.</param>
        /// <returns>A new <see cref="IMessageDescriptor">message descriptor</see>.</returns>
        public virtual IMessageDescriptor GetDescriptor( IOptions options )
        {
            Arg.NotNull( options, nameof( options ) );
            return new CommandDescriptor<TKey>( AggregateId, this, options );
        }
    }
}