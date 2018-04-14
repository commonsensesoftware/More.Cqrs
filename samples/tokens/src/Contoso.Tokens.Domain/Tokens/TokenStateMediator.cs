namespace Contoso.Domain.Tokens
{
    using More.Domain;
    using More.Domain.Commands;
    using System.Threading.Tasks;

    public class TokenStateMediator :
        IHandleCommand<ActivateToken>,
        IHandleCommand<ReserveToken>,
        IHandleCommand<DeactivateToken>,
        IHandleCommand<UnreserveToken>,
        IHandleCommand<RedeemToken>,
        IHandleCommand<VoidToken>
    {
        readonly IRepository<string, Token> repository;
        readonly ITokenVault tokenVault;

        public TokenStateMediator( IRepository<string, Token> repository, ITokenVault tokenVault )
        {
            this.repository = repository;
            this.tokenVault = tokenVault;
        }

        public async Task Handle( ReserveToken command, IMessageContext context )
        {
            var request = new TransferRequest( command.AggregateId, command.BillingAccountId, command.CatalogId, 1, false, command.CorrelationId );
            var reserved = await tokenVault.Transfer( request, context.CancellationToken );

            if ( !reserved )
            {
                // TODO: reply with event that token could not be reserved
            }
        }

        public async Task Handle( RedeemToken command, IMessageContext context )
        {
            var token = await repository.Single( command.AggregateId, context.CancellationToken );
            token.Redeem( command.AccountId );
            await repository.Save( token, command.ExpectedVersion, context.CancellationToken );
        }

        public async Task Handle( VoidToken command, IMessageContext context )
        {
            var token = await repository.Single( command.AggregateId, context.CancellationToken );
            token.Void();
            await repository.Save( token, command.ExpectedVersion, context.CancellationToken );
        }

        public async Task Handle( DeactivateToken command, IMessageContext context )
        {
            var token = await repository.Single( command.AggregateId, context.CancellationToken );
            token.Deactivate( command.OrderId );
            await repository.Save( token, command.ExpectedVersion, context.CancellationToken );
        }

        public async Task Handle( ActivateToken command, IMessageContext context )
        {
            var token = await repository.Single( command.AggregateId, context.CancellationToken );
            token.Activate( command.BillingAccountId, command.OrderId );
            await repository.Save( token, command.ExpectedVersion, context.CancellationToken );
        }

        public async Task Handle( UnreserveToken command, IMessageContext context )
        {
            var token = await repository.Single( command.AggregateId, context.CancellationToken );
            token.Unreserve( command.OrderId );
            await repository.Save( token, command.ExpectedVersion, context.CancellationToken );
        }
    }
}