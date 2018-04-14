namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Events;
    using System;

    public class MintCanceled : Event
    {
        public MintCanceled( Guid aggregateId ) => AggregateId = aggregateId;
    }
}