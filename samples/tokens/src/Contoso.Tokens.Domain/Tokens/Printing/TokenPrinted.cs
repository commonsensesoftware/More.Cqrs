namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Events;
    using System;
    
    public class TokenPrinted : Event
    {
        public TokenPrinted( Guid aggregateId, string tokenId )
        {
            AggregateId = aggregateId;
            TokenId = tokenId;
        }

        public string TokenId { get; }
    }
}