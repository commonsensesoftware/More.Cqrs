// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Commands;
    using System;

    /// <summary>
    /// Defines the behavior of a command that starts a saga.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    public interface IStartWith<in TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
    }
}