// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using More.Domain.Events;

    [ContractClassFor( typeof( ISagaInstance ) )]
    abstract class ISagaInstanceContract : ISagaInstance
    {
        bool ISagaInstance.Completed => default( bool );

        CorrelationProperty ISagaInstance.CorrelationProperty => default( CorrelationProperty );

        DateTimeOffset ISagaInstance.Created => default( DateTimeOffset );

        ISagaData ISagaInstance.Data => default( ISagaData );

        bool ISagaInstance.IsNew => default( bool );

        SagaMetadata ISagaInstance.Metadata
        {
            get
            {
                Contract.Ensures( Contract.Result<SagaMetadata>() != null );
                return null;
            }
        }

        DateTimeOffset ISagaInstance.Modified => default( DateTimeOffset );

        bool ISagaInstance.NotFound => default( bool );

        Guid ISagaInstance.SagaId => default( Guid );

        IReadOnlyList<IEvent> ISagaInstance.UncommittedEvents
        {
            get
            {
                Contract.Ensures( Contract.Result<IReadOnlyList<IEvent>>() != null );
                return null;
            }
        }

        void ISagaInstance.AcceptChanges() { }

        void ISagaInstance.AttachExisting( ISagaData data )
        {
            Contract.Requires<ArgumentNullException>( data != null, nameof( data ) );
        }

        void ISagaInstance.AttachNew( ISagaData data )
        {
            Contract.Requires<ArgumentNullException>( data != null, nameof( data ) );
        }

        void ISagaInstance.Complete() { }

        void ISagaInstance.MarkAsNotFound() { }

        void ISagaInstance.Update() { }

        void ISagaInstance.ValidateChanges() { }
    }
}