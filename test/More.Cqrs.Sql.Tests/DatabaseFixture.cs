namespace More.Domain
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Guid;

    public class DatabaseFixture : IAsyncLifetime
    {
        readonly string MasterConnectionString = @"server=(localdb)\MSSQLLocalDB;database=master;trusted_connection=true;application name=Integration Tests";
        readonly string database = "Test-" + NewGuid().ToString( "n" );

        public DatabaseFixture() => ConnectionString = NewConnectionString( database );

        public Task DisposeAsync() => DropDatabase();

        public Task InitializeAsync() => CreateDatabase();

        public string ConnectionString { get; }

        static string NewConnectionString( string initialCatalog )
        {
            var builder = new SqlConnectionStringBuilder
            {
                ApplicationName = "Integration Tests",
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = initialCatalog,
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,
            };

            return builder.ToString();
        }

        async Task CreateDatabase()
        {
            using var connection = new SqlConnection( MasterConnectionString );
            using var command = connection.CreateCommand();
            
            command.CommandText = $"CREATE DATABASE [{database}];";
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        async Task DropDatabase()
        {
            var statements = new[]
            {
                $"EXECUTE msdb.dbo.sp_delete_database_backuphistory @database_name = N'{database}';",
                $"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;",
                $"DROP DATABASE [{database}];",
            };

            using var connection = new SqlConnection( MasterConnectionString );
            using var command = connection.CreateCommand();
            
            await connection.OpenAsync();

            foreach ( var statement in statements )
            {
                command.CommandText = statement;

                try
                {
                    await command.ExecuteNonQueryAsync();
                }
                catch ( SqlException )
                {
                }
            }
        }
    }
}