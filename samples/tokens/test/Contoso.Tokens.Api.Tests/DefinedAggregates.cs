namespace Contoso.Services
{
    using More.Domain.Events;
    using More.Domain.Persistence;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class DefinedAggregates
    {
        readonly Lazy<AggregateFinder<string>> token = NewAggregateFinder<string>( nameof( Token ) );
        readonly Lazy<AggregateFinder<Guid>> printJob = NewAggregateFinder<Guid>( nameof( PrintJob ) );

        public AggregateFinder<string> Token => token.Value;

        public AggregateFinder<Guid> PrintJob => printJob.Value;

        static Lazy<AggregateFinder<TKey>> NewAggregateFinder<TKey>( string entityName ) =>
            new Lazy<AggregateFinder<TKey>>( () => new AggregateFinder<TKey>( new SqlEventStore<TKey>( No.Persistence, Sql.LocalDb.For( entityName ) ) ) );

        sealed class No : IPersistence
        {
            No() { }

            internal static IPersistence Persistence { get; } = new No();

            public Task Persist( Commit commit, CancellationToken cancellationToken ) => throw new NotImplementedException();
        }

        sealed class Sql
        {
            const string LocalDB = @"server=(localdb)\mssqllocaldb;database=ContosoTokens;trusted_connection=true;application name=Api Tests";

            Sql() { }

            internal static Sql LocalDb { get; } = new Sql() { };

            internal SqlEventStoreConfiguration For( string entityName ) =>
                new SqlEventStoreConfigurationBuilder().HasConnectionString( LocalDB ).HasEntityName( entityName ).CreateConfiguration();
        }
    }
}