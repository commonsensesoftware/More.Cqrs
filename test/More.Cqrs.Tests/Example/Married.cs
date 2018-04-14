namespace More.Domain.Example
{
    using More.Domain.Events;
    using System;

    public class Married : Event
    {
        public Married( Guid id, Guid spouseId, DateTimeOffset date )
        {
            AggregateId = id;
            SpouseId = spouseId;
            Date = date;
        }

        public Guid SpouseId { get; }

        public DateTimeOffset Date { get; }
    }
}
