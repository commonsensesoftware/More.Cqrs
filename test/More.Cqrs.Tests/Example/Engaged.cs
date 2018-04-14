namespace More.Domain.Example
{
    using More.Domain.Events;
    using System;

    public class Engaged : Event
    {
        public Engaged( Guid id, Guid fianceId, DateTimeOffset date )
        {
            AggregateId = id;
            FiancéId = fianceId;
            Date = date;
        }

        public Guid FiancéId { get; }

        public DateTimeOffset Date { get; }
    }
}
