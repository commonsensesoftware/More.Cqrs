namespace Contoso.Services.Components
{
    using More.Composition;
    using More.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Tokens;
    using static DefaultConfigurationSettings;

    public sealed class ReadModel :
        DbContext,
        IReadOnlyRepository<Token>,
        IReadOnlyRepository<PrintJob>,
        IReadOnlyRepository<Order>,
        IReadOnlyRepository<MintRequest>
    {
        public ReadModel( [Setting( ConnectionStringKey )] string connectionString ) : base( connectionString )
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.SetInitializer<ReadModel>( null );
        }

        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            modelBuilder.HasDefaultSchema( "Tokens" );

            modelBuilder.Entity<MintRequest>()
                        .ToTable( nameof( MintRequest ) )
                        .HasKey( mr => mr.Id )
                        .Property( mr => mr.Id )
                        .HasColumnName( "MintRequestId" );

            var order = modelBuilder.Entity<Order>();

            order.ToTable( nameof( Order ) )
                 .HasKey( o => o.Id )
                 .Property( o => o.Id )
                 .HasColumnName( "OrderId" );

            order.HasMany( o => o.Tokens )
                 .WithMany()
                 .Map( m => m.ToTable( "OrderLineItem" ).MapLeftKey( "OrderId" ).MapRightKey( "TokenId" ) );

            var printJob = modelBuilder.Entity<PrintJob>();

            printJob.ToTable( nameof( PrintJob ) )
                    .HasKey( o => o.Id )
                    .Property( o => o.Id )
                    .HasColumnName( "PrintJobId" );

            printJob.HasMany( o => o.Tokens )
                    .WithMany()
                    .Map( m => m.ToTable( "SpooledToken" ).MapLeftKey( "PrintJobId" ).MapRightKey( "TokenId" ) );

            modelBuilder.Entity<Token>()
                        .ToTable( nameof( Token ) )
                        .HasKey( t => t.Id )
                        .Property( t => t.Id )
                        .HasColumnName( "TokenId" );
        }

        public async Task<IEnumerable<Token>> GetAsync( Func<IQueryable<Token>, IQueryable<Token>> queryShaper, CancellationToken cancellationToken ) =>
            await queryShaper( Set<Token>() ).ToArrayAsync( cancellationToken ).ConfigureAwait( false );

        public async Task<TResult> GetAsync<TResult>( Func<IQueryable<Token>, TResult> queryShaper, CancellationToken cancellationToken ) =>
            await Task<TResult>.Factory.StartNew( () => queryShaper( Set<Token>() ), cancellationToken ).ConfigureAwait( false );

        public async Task<IEnumerable<PrintJob>> GetAsync( Func<IQueryable<PrintJob>, IQueryable<PrintJob>> queryShaper, CancellationToken cancellationToken ) =>
            await queryShaper( Set<PrintJob>() ).ToArrayAsync( cancellationToken ).ConfigureAwait( false );

        public async Task<TResult> GetAsync<TResult>( Func<IQueryable<PrintJob>, TResult> queryShaper, CancellationToken cancellationToken ) =>
            await Task<TResult>.Factory.StartNew( () => queryShaper( Set<PrintJob>() ), cancellationToken ).ConfigureAwait( false );

        public async Task<IEnumerable<Order>> GetAsync( Func<IQueryable<Order>, IQueryable<Order>> queryShaper, CancellationToken cancellationToken ) =>
            await queryShaper( Set<Order>() ).ToArrayAsync( cancellationToken ).ConfigureAwait( false );

        public async Task<TResult> GetAsync<TResult>( Func<IQueryable<Order>, TResult> queryShaper, CancellationToken cancellationToken ) =>
            await Task<TResult>.Factory.StartNew( () => queryShaper( Set<Order>() ), cancellationToken ).ConfigureAwait( false );

        public async Task<IEnumerable<MintRequest>> GetAsync( Func<IQueryable<MintRequest>, IQueryable<MintRequest>> queryShaper, CancellationToken cancellationToken ) =>
            await queryShaper( Set<MintRequest>() ).ToArrayAsync( cancellationToken ).ConfigureAwait( false );

        public async Task<TResult> GetAsync<TResult>( Func<IQueryable<MintRequest>, TResult> queryShaper, CancellationToken cancellationToken ) =>
            await Task<TResult>.Factory.StartNew( () => queryShaper( Set<MintRequest>() ), cancellationToken ).ConfigureAwait( false );
    }
}