// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Commands
{
    using System;

    /// <summary>
    /// Represents the base implementation for a command.
    /// </summary>
    public abstract class Command : Command<Guid>
    {
    }
}