
namespace Contoso.Domain.Tokens
{
    using More.Domain.Events;
    using System;

    public class TokenCirculated : Event<string>
    {
        public TokenCirculated( string aggregateId ) => AggregateId = aggregateId;
    }
}