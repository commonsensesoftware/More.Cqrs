namespace More.Domain.Events
{
    using More.Domain.Persistence;

    public class EventStoreFixture<TKey>
    {
        public EventStoreFixture( DatabaseFixture database )
        {
            var builder = new IsamEventStoreConfigurationBuilder();

            Database = database;
            Configuration = builder.HasConnectionString( database.ConnectionString ).CreateConfiguration();
            Persistence = EventStoreOnlyPersistence.New( Configuration, database );
            Configuration.CreateTables( typeof( TKey ) );
        }

        protected DatabaseFixture Database { get; }

        public IPersistence Persistence { get; }

        public IsamEventStoreConfiguration Configuration { get; }
    }
}