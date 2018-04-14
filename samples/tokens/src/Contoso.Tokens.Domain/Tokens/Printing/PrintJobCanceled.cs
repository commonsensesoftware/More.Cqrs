namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain.Events;
    using System;

    public class PrintJobCanceled : Event
    {
        public PrintJobCanceled( Guid aggregateId ) => AggregateId = aggregateId;
    }
}