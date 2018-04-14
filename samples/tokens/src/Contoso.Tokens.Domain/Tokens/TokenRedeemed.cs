
namespace Contoso.Domain.Tokens
{
    using More.Domain.Events;
    using System;

    public class TokenRedeemed : Event<string>
    {
        public TokenRedeemed( string aggregateId, string accountId )
        {
            AggregateId = aggregateId;
            AccountId = accountId;
        }

        public string AccountId { get; }
    }
}