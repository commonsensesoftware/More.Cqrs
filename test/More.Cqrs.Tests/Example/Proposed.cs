namespace More.Domain.Example
{
    using More.Domain.Events;
    using System;

    public class Proposed : Event
    {
        public Proposed( Guid id, Guid fianceId, DateTimeOffset date )
        {
            AggregateId = id;
            FianceId = fianceId;
            Date = date;
        }

        public Guid FianceId { get; }

        public DateTimeOffset Date { get; }
    }
}
