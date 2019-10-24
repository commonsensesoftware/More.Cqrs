// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Messaging;
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents an object that correlates a message property to a saga property.
    /// </summary>
    /// <typeparam name="TData">The type of saga data.</typeparam>
    public class SagaCorrelator<TData> where TData : ISagaData
    {
        readonly ICorrelateSagaToMessage correlation;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaCorrelator{TData}"/> class.
        /// </summary>
        /// <param name="correlation">The underlying <see cref="ICorrelateSagaToMessage">correlation</see>.</param>
        public SagaCorrelator( ICorrelateSagaToMessage correlation ) => this.correlation = correlation;

        /// <summary>
        /// Correlates the specified message property to a saga property.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <param name="messageProperty">The <see cref="Expression{T}">expression</see> representing the message property to correlate.</param>
        /// <returns>A saga data property <see cref="ToExpression{TData,TMessage}">expression</see> to correlate with.</returns>
        public ToExpression<TData, TMessage> Correlate<TMessage>( Expression<Func<TMessage, object>> messageProperty ) where TMessage : IMessage =>
            new ToExpression<TData, TMessage>( correlation, messageProperty );
    }
}