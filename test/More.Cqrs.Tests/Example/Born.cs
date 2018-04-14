namespace More.Domain.Example
{
    using More.Domain.Events;
    using System;

    public class Born : Event
    {
        public Born( Guid id, string firstName, string lastName, DateTimeOffset date )
        {
            AggregateId = id;
            FirstName = firstName;
            LastName = lastName;
            Date = date;
        }

        public string FirstName { get; }

        public string LastName { get; }

        public DateTimeOffset Date { get; }
    }
}