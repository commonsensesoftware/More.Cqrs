namespace Contoso.Domain.Simulators
{
    using Contoso.Domain.Tokens;
    using More.Domain;
    using More.Domain.Events;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;
    using ITokenVault = Tokens.ITokenVault;
    using ReadModel = Contoso.Tokens;

    class TokenVault : ITokenVault
    {
        readonly IRepository<string, Token> repository;
        readonly IReadOnlyList<ReadModel.Token> tokens;
        readonly Dictionary<Guid, List<Token>> deposited = new Dictionary<Guid, List<Token>>();
        readonly ConcurrentDictionary<Guid, HashSet<string>> vault = new ConcurrentDictionary<Guid, HashSet<string>>();

        internal TokenVault( IRepository<string, Token> repository, IReadOnlyList<ReadModel.Token> tokens )
        {
            this.repository = repository;
            this.tokens = tokens;
        }

        public async Task Deposit( IEnumerable<Token> tokens, CancellationToken cancellationToken )
        {
            using ( var iterator = tokens.GetEnumerator() )
            {
                if ( !iterator.MoveNext() )
                {
                    return;
                }

                var token = iterator.Current;
                var mintRequestId = TokenIdentifier.Parse( token.Id ).MintRequestId;
                var list = new List<Token>() { token };

                await repository.Save( token, ExpectedVersion.Initial, cancellationToken );

                while ( iterator.MoveNext() )
                {
                    token = iterator.Current;
                    await repository.Save( token, ExpectedVersion.Initial, cancellationToken );
                    list.Add( token );
                }

                deposited.Add( mintRequestId, list );
            }
        }

        public async Task ReleaseToCirculation( Guid mintRequestId, string correlationId, CancellationToken cancellationToken )
        {
            if ( !deposited.TryGetValue( mintRequestId, out var tokens ) )
            {
                return;
            }

            foreach ( var token in tokens )
            {
                token.Circulate();
                await repository.Save( token, token.Version, cancellationToken );
            }

            deposited.Remove( mintRequestId );
        }

        public Task RemoveFromCirculation( Guid mintRequestId, string correlationId, CancellationToken cancellationToken )
        {
            foreach ( var circulated in vault.Select( p => p.Value ) )
            {
                var tokenIds = circulated.ToArray();

                foreach ( var tokenId in tokenIds )
                {
                    var id = TokenIdentifier.Parse( tokenId );

                    if ( id.MintRequestId == mintRequestId )
                    {
                        circulated.Remove( tokenId );
                    }
                }
            }

            return CompletedTask;
        }

        public async Task<bool> Transfer( TransferRequest request, CancellationToken cancellationToken )
        {
            var lot = tokens.Where( t => t.CatalogId == request.CatalogId && !IsReserved( t.Id ) );
            var allocation = vault.GetOrAdd( request.OrderId, key => new HashSet<string>() );

            foreach ( var token in lot )
            {
                var aggregate = await repository.Single( token.Id, cancellationToken );

                aggregate.Reserve( request.OrderId, request.BillingAccountId );
                await repository.Save( aggregate, aggregate.Version, cancellationToken );

                if ( request.ActivateImmediately )
                {
                    aggregate.Activate( request.BillingAccountId, request.OrderId );
                    await repository.Save( aggregate, aggregate.Version, cancellationToken );
                }

                allocation.Add( token.Id );
            }

            return true;
        }

        public async Task ReverseTransfer( Guid orderId, string correlationId, CancellationToken cancellationToken )
        {
            var allocation = vault.GetOrAdd( orderId, key => new HashSet<string>() );

            foreach ( var id in allocation )
            {
                var aggregate = await repository.Single( id, cancellationToken );
                aggregate.Unreserve( orderId );
                await repository.Save( aggregate, aggregate.Version, cancellationToken );
            }
        }

        bool IsReserved( string tokenId ) => vault.Values.Any( v => v.Contains( tokenId ) );
    }
}