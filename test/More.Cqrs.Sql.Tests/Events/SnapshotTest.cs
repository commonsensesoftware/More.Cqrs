namespace More.Domain.Events
{
    using FluentAssertions;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Guid;

    public class SnapshotTest : DatabaseTest<SnapshotFixture<Guid>>
    {
        public SnapshotTest( SnapshotFixture<Guid> setup ) : base( setup )
        {
            EventStore = new SqlEventStore( setup.Persistence, setup.Configuration );
            Snapshots = new SqlSnapshotStore( setup.Configuration );
        }

        SqlEventStore EventStore { get; }

        SqlSnapshotStore Snapshots { get; }

        [Fact]
        public async Task load_should_retrieve_saved_snapshot()
        {
            // arrange
            var bankAccount = new BankAccount( NewGuid() );

            bankAccount.Credit( 100m );
            bankAccount.Debit( 20m );
            bankAccount.Debit( 20m );
            bankAccount.Debit( 10m );
            bankAccount.Credit( 100m );

            await EventStore.Save( bankAccount.Id, bankAccount.UncommittedEvents, ExpectedVersion.Initial, CancellationToken.None );
            bankAccount.AcceptChanges();

            var savedSnapshot = bankAccount.CreateSnapshot();
            await Snapshots.Save( savedSnapshot, CancellationToken.None );
            var loadedSnapshot = default( ISnapshot<Guid> );

            // act
            using ( var connection = Setup.Configuration.CreateConnection() )
            {
                await connection.OpenAsync();
                var descriptor = await Snapshots.Load( connection, bankAccount.Id, CancellationToken.None );
                loadedSnapshot = Setup.Configuration.NewMessageSerializer<AccountSummary>().Deserialize( descriptor.SnapshotType, 1, descriptor.Snapshot );
            }

            // assert
            loadedSnapshot.Should().BeEquivalentTo( savedSnapshot );
        }

        [Fact]
        public async Task load_should_retrieve_events_without_snapshot()
        {
            // arrange
            var bankAccount = new BankAccount( NewGuid() );

            bankAccount.Credit( 100m );
            bankAccount.Debit( 20m );

            await EventStore.Save( bankAccount.Id, bankAccount.UncommittedEvents, ExpectedVersion.Initial, CancellationToken.None );
            bankAccount.AcceptChanges();

            // act
            var events = await EventStore.Load( bankAccount.Id, CancellationToken.None );

            // assert
            events.Should().BeEquivalentTo(
                new IEvent[]
                {
                    new AccountTransaction( bankAccount.Id, 100m ) { Version = 0, Sequence = 0 },
                    new AccountTransaction( bankAccount.Id, -20m ) { Version = 0, Sequence = 1 }
                } );
        }

        [Fact]
        public async Task load_should_retrieve_snapshot_without_further_events()
        {
            // arrange
            var bankAccount = new BankAccount( NewGuid() );

            bankAccount.Credit( 100m );
            bankAccount.Debit( 20m );
            bankAccount.Debit( 20m );

            await EventStore.Save( bankAccount.Id, bankAccount.UncommittedEvents, ExpectedVersion.Initial, CancellationToken.None );
            bankAccount.AcceptChanges();

            var savedSnapshot = bankAccount.CreateSnapshot();
            await Snapshots.Save( savedSnapshot, CancellationToken.None );

            // act
            var events = await EventStore.Load( bankAccount.Id, CancellationToken.None );

            // assert
            events.Should().BeEquivalentTo( new IEvent[] { new AccountSummary( bankAccount.Id, 0, 60m ) } );
        }

        [Fact]
        public async Task load_should_retrieve_events_with_snapshot()
        {
            // arrange
            var bankAccount = new BankAccount( NewGuid() );

            bankAccount.Credit( 100m );
            bankAccount.Debit( 20m );
            bankAccount.Debit( 20m );

            await EventStore.Save( bankAccount.Id, bankAccount.UncommittedEvents, ExpectedVersion.Initial, CancellationToken.None );
            bankAccount.AcceptChanges();

            var savedSnapshot = bankAccount.CreateSnapshot();
            await Snapshots.Save( savedSnapshot, CancellationToken.None );

            bankAccount.Debit( 10m );
            bankAccount.Credit( 100m );

            await EventStore.Save( bankAccount.Id, bankAccount.UncommittedEvents, bankAccount.Version, CancellationToken.None );
            bankAccount.AcceptChanges();

            // act
            var events = await EventStore.Load( bankAccount.Id, CancellationToken.None );

            // assert
            events.Should().BeEquivalentTo(
                new IEvent[]
                {
                    new AccountSummary( bankAccount.Id, 0, 60m ),
                    new AccountTransaction( bankAccount.Id, -10m ) { Version = 1, Sequence = 0 },
                    new AccountTransaction( bankAccount.Id, 100m ) { Version = 1, Sequence = 1 }
                } );
        }
    }
}