namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Events;
    using System;

    public class PrintJobSpooled : Event
    {
        public PrintJobSpooled( Guid aggregateId ) => AggregateId = aggregateId;
    }
}