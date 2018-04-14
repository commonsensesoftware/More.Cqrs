namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Events;
    using System;
    
    public class TokenSpooled : Event
    {
        public TokenSpooled( Guid aggregateId, string tokenId, int tokenVersion, string tokenCode, string tokenHash )
        {
            AggregateId = aggregateId;
            TokenId = tokenId;
            TokenVersion = tokenVersion;
            TokenCode = tokenCode;
            TokenHash = tokenHash;
        }

        public string TokenId { get; }

        public string TokenCode { get; }

        public string TokenHash { get; }

        public int TokenVersion { get; }
    }
}