// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using More.Domain.Messaging;
    using More.Domain.Options;
    using static Domain.Options.DefaultOptions;

    /// <summary>
    /// Represents a message descriptor for a command.
    /// </summary>
    /// <typeparam name="TKey">The key type for the command.</typeparam>
    public class CommandDescriptor<TKey> : MessageDescriptor<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandDescriptor{TKey}"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier associated with the command.</param>
        /// <param name="command">The <see cref="ICommand">command</see> the descriptor is for.</param>
        public CommandDescriptor( TKey aggregateId, ICommand command ) : base( aggregateId, command, None ) => Command = command;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandDescriptor{TKey}"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier associated with the command.</param>
        /// <param name="command">The <see cref="ICommand">command</see> the descriptor is for.</param>
        /// <param name="options">The <see cref="IOptions">options</see> associated with the <paramref name="command"/>.</param>
        public CommandDescriptor( TKey aggregateId, ICommand command, IOptions options ) : base( aggregateId, command, options ) => Command = command;

        /// <summary>
        /// Gets the unique message identifier.
        /// </summary>
        /// <value>The unique message identifier.</value>
        public override string MessageId => $"{AggregateId}.{Command.ExpectedVersion}";

        /// <summary>
        /// Gets the described command.
        /// </summary>
        /// <value>The described <see cref="ICommand">command</see>.</value>
        public ICommand Command { get; }
    }
}