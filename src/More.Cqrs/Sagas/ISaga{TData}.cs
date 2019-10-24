// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;

    /// <summary>
    /// Defines the behavior of a saga.
    /// </summary>
    /// <typeparam name="TData">The type of saga data.</typeparam>
    public interface ISaga<TData> : IAggregate<Guid> where TData : notnull, ISagaData
    {
        /// <summary>
        /// Gets or sets the associated saga data.
        /// </summary>
        /// <value>The associated <see cref="ISagaData">saga data</see>.</value>
        TData Data { get; set; }

        /// <summary>
        /// Gets a value indicating whether the saga is complete.
        /// </summary>
        /// <value>True if the saga is complete; otherwise, false.</value>
        bool Completed { get; }

        /// <summary>
        /// Correlates the saga using the specified correlation information.
        /// </summary>
        /// <param name="correlation">The <see cref="ICorrelateSagaToMessage">correlation information</see> used to correlate the saga.</param>
        void CorrelateUsing( ICorrelateSagaToMessage correlation );
    }
}