namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Commands;
    using System;

    public class PrintToken : Command
    {
        public PrintToken( Guid aggregateId, string tokenId )
        {
            AggregateId = aggregateId;
            TokenId = tokenId;
        }

        public string TokenId { get; }
    }
}