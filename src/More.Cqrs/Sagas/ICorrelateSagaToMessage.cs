// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines the behavior of an object that correlates sagas to messages.
    /// </summary>
    public interface ICorrelateSagaToMessage
    {
        /// <summary>
        /// Configures the correlation between the specified saga and message properties.
        /// </summary>
        /// <typeparam name="TData">The type of saga property.</typeparam>
        /// <typeparam name="TMessage">The type of message property.</typeparam>
        /// <param name="sagaDataProperty">The <see cref="Expression{T}">expression</see> representing the correlated saga property.</param>
        /// <param name="messageProperty">The <see cref="Expression{T}">expression</see> representing the correlated message property.</param>
        void Configure<TData, TMessage>( Expression<Func<TData, object>> sagaDataProperty, Expression<Func<TMessage, object>> messageProperty );
    }
}