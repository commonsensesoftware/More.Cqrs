namespace More.Domain.Events
{
    using System;

    public class BankAccount : Aggregate
    {
        decimal balance;

        public BankAccount( Guid id ) => Id = id;

        public void Debit( decimal amount ) => Record( new AccountTransaction( Id, -amount ) );

        public void Credit( decimal amount ) => Record( new AccountTransaction( Id, amount ) );

        void Apply( AccountTransaction @event ) => balance += @event.Amount;

        void Apply( AccountSummary snapshot ) => balance = snapshot.Balance;

        public override ISnapshot<Guid> CreateSnapshot() => new AccountSummary( Id, Version, balance );
    }
}