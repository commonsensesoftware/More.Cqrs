namespace More.Domain.Events
{
    using More.Domain.Persistence;

    public class SnapshotFixture<TKey>
    {
        public SnapshotFixture( DatabaseFixture database )
        {
            var builder = new IsamEventStoreConfigurationBuilder();

            Database = database;
            Configuration = builder.HasConnectionString( database.ConnectionString ).SupportsSnapshots().CreateConfiguration();
            Persistence = EventStoreOnlyPersistence.New( Configuration, database );
            Configuration.CreateTables( typeof( TKey ) );
        }

        protected DatabaseFixture Database { get; }

        public IsamEventStoreConfiguration Configuration { get; }

        public IPersistence Persistence { get; }
    }
}