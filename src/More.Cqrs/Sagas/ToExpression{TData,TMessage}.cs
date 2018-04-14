// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using More.Domain.Messaging;
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents the expression to correlate a saga data property.
    /// </summary>
    /// <typeparam name="TData">The type of saga data.</typeparam>
    /// <typeparam name="TMessage">The type of message.</typeparam>
    public class ToExpression<TData, TMessage>
        where TData : ISagaData
        where TMessage : IMessage
    {
        readonly ICorrelateSagaToMessage correlation;
        readonly Expression<Func<TMessage, object>> messageProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToExpression{TData,TMessage}"/> class.
        /// </summary>
        /// <param name="correlation">The underlying <see cref="ICorrelateSagaToMessage">correlation</see>.</param>
        /// <param name="messageProperty">The message property <see cref="Expression{T}">expression</see> the saga property is correlated with.</param>
        public ToExpression( ICorrelateSagaToMessage correlation, Expression<Func<TMessage, object>> messageProperty )
        {
            Arg.NotNull( correlation, nameof( correlation ) );
            Arg.NotNull( messageProperty, nameof( messageProperty ) );

            this.correlation = correlation;
            this.messageProperty = messageProperty;
        }

        /// <summary>
        /// Defines the saga property the expression is correlated to.
        /// </summary>
        /// <param name="sagaDataProperty">The saga property <see cref="Expression{T}">expression</see> to correlate to.</param>
        public void To( Expression<Func<TData, object>> sagaDataProperty )
        {
            Arg.NotNull( sagaDataProperty, nameof( sagaDataProperty ) );
            correlation.Configure( sagaDataProperty, messageProperty );
        }
    }
}