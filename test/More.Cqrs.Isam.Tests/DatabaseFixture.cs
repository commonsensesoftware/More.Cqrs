namespace More.Domain
{
    using System;
    using System.IO;
    using static System.Guid;
    using static System.IO.Path;

    public class DatabaseFixture : IDisposable
    {
        bool disposed;

        public DatabaseFixture() => CreateDatabase();

        public string ConnectionString { get; } = NewConnectionString();

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            DropDatabase();
        }

        static string NewConnectionString()
        {
            var path = GetFullPath( NewGuid().ToString( "n" ) );

            if ( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }

            return Combine( path, "test.edb" );
        }

        void CreateDatabase()
        {
            var connection = new IsamConnection( ConnectionString );

            using ( var instance = connection.Open() )
            using ( var session = instance.CreateSession() )
            {
                session.CreateDatabase( ConnectionString );
                session.AttachDatabase( connection.DatabaseName );
                session.DetachDatabase( connection.DatabaseName );
            }
        }

        void DropDatabase()
        {
            var file = new FileInfo( ConnectionString );
            var path = file.DirectoryName;

            if ( Directory.Exists( path ) )
            {
                Directory.Delete( path, recursive: true );
            }
        }
    }
}