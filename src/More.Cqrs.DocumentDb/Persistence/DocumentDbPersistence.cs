// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Persistence
{
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using More.Domain.Events;
    using More.Domain.Messaging;
    using More.Domain.Persistence;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents persistence backed by Microsoft Document DB.
    /// </summary>
    public class DocumentDbPersistence : IPersistence
    {
        /// <summary>
        /// Persists the specified commit.
        /// </summary>
        /// <param name="commit">The <see cref="Commit">commit</see> to append.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">token</see> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchronous operation.</returns>
        public Task Persist( Commit commit, CancellationToken cancellationToken )
        {
            // TODO: implementation
            throw new NotImplementedException();

            // using ( var iterator = commit.Events.GetEnumerator() )
            // {
            //     if ( !iterator.MoveNext() )
            //     {
            //         return;
            //     }
            //     var documentCollectionUri = Configuration.CollectionLink;
            //     do
            //     {
            //         var @event = iterator.Current;
            //         var eventDescriptor = new EventDescriptor<object>( aggregateId, @event );
            //         IMessageDescriptor messageDescriptor = eventDescriptor;
            //         var recordedEvent = new RecordedEvent<TKey>()
            //         {
            //             Id = aggregateId,
            //             Version = eventDescriptor.Event.Version,
            //             Type = messageDescriptor.MessageType,
            //             Revision = eventDescriptor.Event.Revision,
            //             Message = eventSerializer.Serialize( eventDescriptor.Event ),
            //         };
            //         // TODO: add support to handle duplicate keys
            //         // TODO: add batch support (a la stored procedure)?
            //         await client.CreateDocumentAsync( documentCollectionUri, recordedEvent ).ConfigureAwait( false );
            //     }
            //     while ( iterator.MoveNext() );
            // }
        }
    }
}