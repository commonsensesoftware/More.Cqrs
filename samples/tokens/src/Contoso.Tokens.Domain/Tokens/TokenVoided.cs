
namespace Contoso.Domain.Tokens
{
    using More.Domain.Events;
    using System;

    public class TokenVoided : Event<string>
    {
        public TokenVoided( string aggregateId ) => AggregateId = aggregateId;
    }
}