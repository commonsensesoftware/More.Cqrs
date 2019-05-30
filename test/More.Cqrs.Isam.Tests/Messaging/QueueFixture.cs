namespace More.Domain.Messaging
{
    public class QueueFixture
    {
        public QueueFixture( DatabaseFixture database )
        {
            var builder = new IsamMessageQueueConfigurationBuilder();

            Database = database;
            Configuration = builder.HasConnectionString( database.ConnectionString ).CreateConfiguration();
            Configuration.CreateTables();
        }

        protected DatabaseFixture Database { get; }

        public IsamMessageQueueConfiguration Configuration { get; }
    }
}