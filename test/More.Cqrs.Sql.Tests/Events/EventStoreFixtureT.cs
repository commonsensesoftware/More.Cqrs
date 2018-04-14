namespace More.Domain.Events
{
    using More.Domain.Persistence;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Threading.Tasks.Task;

    public class EventStoreFixture<TKey> : IAsyncLifetime
    {
        public EventStoreFixture( DatabaseFixture database )
        {
            var builder = new SqlEventStoreConfigurationBuilder();

            Database = database;
            Configuration = builder.HasConnectionString( database.ConnectionString ).CreateConfiguration();
            Persistence = EventStoreOnlyPersistence.New( Configuration, database );
        }

        protected DatabaseFixture Database { get; }

        public IPersistence Persistence { get; }

        public SqlEventStoreConfiguration Configuration { get; }

        public Task DisposeAsync() => CompletedTask;

        public Task InitializeAsync() => Configuration.CreateTables( typeof( TKey ) );
    }
}