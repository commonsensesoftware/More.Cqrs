namespace More.Domain.Sagas
{
    using Xunit;

    public class SagaFixture
    {
        public SagaFixture( DatabaseFixture database )
        {
            var builder = new IsamSagaStorageConfigurationBuilder();

            Database = database;
            Configuration = builder.HasConnectionString( database.ConnectionString ).CreateConfiguration();
            Configuration.CreateTables();
        }

        protected DatabaseFixture Database { get; }

        public IsamSagaStorageConfiguration Configuration { get; }
    }
}