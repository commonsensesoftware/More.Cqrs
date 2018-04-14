namespace More.Domain.Events
{
    using System;

    public class AccountSummary : SnapshotEvent<Guid>
    {
        public AccountSummary( Guid aggregateId, int version, decimal balance )
        {
            Id = aggregateId;
            Version = version;
            Balance = balance;
        }

        public decimal Balance { get; }
    }
}