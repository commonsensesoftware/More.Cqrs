// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using Messaging;

    /// <summary>
    /// Defines the behavior of a command.
    /// </summary>
    public interface ICommand : IMessage
    {
        /// <summary>
        /// Gets or sets the associated aggregate version.
        /// </summary>
        /// <value>The version of the aggregate when the command was created.</value>
        int ExpectedVersion { get; set; }
    }
}