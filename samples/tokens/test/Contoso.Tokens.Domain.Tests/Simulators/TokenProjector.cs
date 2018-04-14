namespace Contoso.Domain.Simulators
{
    using Contoso.Domain.Tokens;
    using More.Domain;
    using More.Domain.Events;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static Contoso.Tokens.TokenState;
    using static System.Threading.Tasks.Task;
    using Token = Contoso.Tokens.Token;

    class TokenProjector :
        IReceiveEvent<TokenMinted>,
        IReceiveEvent<TokenCirculated>,
        IReceiveEvent<TokenReserved>,
        IReceiveEvent<TokenActivated>,
        IReceiveEvent<TokenDeactivated>,
        IReceiveEvent<TokenUnreserved>,
        IReceiveEvent<TokenRedeemed>,
        IReceiveEvent<TokenVoided>
    {
        readonly List<Token> tokens = new List<Token>();

        public IReadOnlyList<Token> Tokens => tokens;

        public Task Receive( TokenDeactivated @event, IMessageContext context )
        {
            var token = tokens.SingleOrDefault( t => t.Id == @event.AggregateId );

            if ( token != null )
            {
                token.State = token.ReservedByBillingAccountId == null ? Minted : Reserved;
                token.Version = @event.Version;
            }

            return CompletedTask;
        }

        public Task Receive( TokenVoided @event, IMessageContext context )
        {
            var token = tokens.SingleOrDefault( t => t.Id == @event.AggregateId );

            if ( token != null )
            {
                token.State = Voided;
                token.Version = @event.Version;
            }

            return CompletedTask;
        }

        public Task Receive( TokenRedeemed @event, IMessageContext context )
        {
            var token = tokens.SingleOrDefault( t => t.Id == @event.AggregateId );

            if ( token != null )
            {
                token.RedeemedByAccountId = @event.AccountId;
                token.State = Redeemed;
                token.Version = @event.Version;
            }

            return CompletedTask;
        }

        public Task Receive( TokenActivated @event, IMessageContext context )
        {
            var token = tokens.SingleOrDefault( t => t.Id == @event.AggregateId );

            if ( token != null )
            {
                token.State = Activated;
                token.Version = @event.Version;
            }

            return CompletedTask;
        }

        public Task Receive( TokenReserved @event, IMessageContext context )
        {
            var token = tokens.SingleOrDefault( t => t.Id == @event.AggregateId );

            if ( token != null )
            {
                token.ReservedByBillingAccountId = @event.BillingAccountId;
                token.State = Reserved;
                token.Version = @event.Version;
            }

            return CompletedTask;
        }

        public Task Receive( TokenUnreserved @event, IMessageContext context )
        {
            var token = tokens.SingleOrDefault( t => t.Id == @event.AggregateId );

            if ( token != null )
            {
                token.ReservedByBillingAccountId = null;
                token.State = Minted;
                token.Version = @event.Version;
            }

            return CompletedTask;
        }

        public Task Receive( TokenMinted @event, IMessageContext context )
        {
            var token = tokens.SingleOrDefault( t => t.Id == @event.AggregateId );

            if ( token == null )
            {
                tokens.Add(
                    new Token()
                    {
                        Id = @event.AggregateId,
                        CatalogId = @event.CatalogId,
                        Code = @event.Code,
                        Hash = @event.Hash,
                        State = Minted,
                        Version = @event.Version
                    } );
            }

            return CompletedTask;
        }

        public Task Receive( TokenCirculated @event, IMessageContext context )
        {
            var token = tokens.SingleOrDefault( t => t.Id == @event.AggregateId );

            if ( token != null )
            {
                token.State = Circulated;
                token.Version = @event.Version;
            }

            return CompletedTask;
        }
    }
}