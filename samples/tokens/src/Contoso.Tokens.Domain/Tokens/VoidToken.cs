namespace Contoso.Domain.Tokens
{
    using More.Domain.Commands;
    using System;

    public class VoidToken : Command<string>
    {
        public VoidToken( string aggregateId, int expectedVersion )
        {
            AggregateId = aggregateId;
            ExpectedVersion = expectedVersion;
        }
    }
}