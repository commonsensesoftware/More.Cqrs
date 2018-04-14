namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Events;
    using System;

    public class Minted : Event
    {
        public Minted( Guid aggregateId ) => AggregateId = aggregateId;
    }
}