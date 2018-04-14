namespace More.Domain.Example
{
    using More.Domain.Events;
    using System;

    public class LastNameChanged : Event
    {
        public LastNameChanged( Guid id, string newLastName )
        {
            AggregateId = id;
            NewLastName = newLastName;
        }

        public string NewLastName { get; }
    }
}