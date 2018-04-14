namespace Contoso.Domain.Tokens
{
    using More.Domain.Commands;
    using System;

    public class RedeemToken : Command<string>
    {
        public RedeemToken( string aggregateId, int expectedVersion, string accountId )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
            AccountId = accountId;
        }

        public string AccountId { get; }
    }
}