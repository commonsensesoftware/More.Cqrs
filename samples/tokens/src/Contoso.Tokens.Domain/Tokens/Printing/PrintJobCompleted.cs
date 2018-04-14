namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Events;
    using System;

    public class PrintJobCompleted : Event
    {
        public PrintJobCompleted( Guid aggregateId ) => AggregateId = aggregateId;
    }
}