namespace More.Domain.Events
{
    using System;

    public class AccountTransaction : Event
    {
        public AccountTransaction( Guid aggregateId, decimal amount )
        {
            AggregateId = aggregateId;
            Amount = amount;
        }

        public decimal Amount { get; }
    }
}