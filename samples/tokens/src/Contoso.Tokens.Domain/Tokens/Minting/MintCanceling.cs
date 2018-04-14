namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MintCanceling : Event
    {
        public MintCanceling( Guid aggregateId, IEnumerable<int> mintJobIds )
        {
            AggregateId = aggregateId;
            MintJobIds = mintJobIds.ToArray();
        }

        public IReadOnlyList<int> MintJobIds { get; }
    }
}