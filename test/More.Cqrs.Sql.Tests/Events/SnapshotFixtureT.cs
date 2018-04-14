namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Threading.Tasks.Task;

    public class SnapshotFixture<TKey> : IAsyncLifetime
    {
        public SnapshotFixture( DatabaseFixture database )
        {
            var builder = new SqlEventStoreConfigurationBuilder();

            Database = database;
            Configuration = builder.HasConnectionString( database.ConnectionString ).SupportsSnapshots().CreateConfiguration();
            Persistence = EventStoreOnlyPersistence.New( Configuration, database );
        }

        protected DatabaseFixture Database { get; }

        public SqlEventStoreConfiguration Configuration { get; }

        public IPersistence Persistence { get; }

        public Task DisposeAsync() => CompletedTask;

        public Task InitializeAsync() => Configuration.CreateTables( typeof( TKey ) );
    }
}