namespace Contoso.Domain.Tokens.Printing
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Events;
    using More.Domain.Sagas;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static PrintingState;
    using static System.Threading.Tasks.Task;

    public class PrintJob : Saga<PrintJobData>,
        IStartWith<Print>,
        IReceiveEvent<TokenReserved>,
        IHandleCommand<PrintToken>,
        IReceiveEvent<TokenDeactivated>,
        IHandleCommand<CancelPrintJob>
    {
        protected override void CorrelateUsing( SagaCorrelator<PrintJobData> correlator )
        {
            correlator.Correlate<Print>( command => command.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<TokenReserved>( @event => @event.OrderId ).To( saga => saga.Id );
            correlator.Correlate<PrintToken>( command => command.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<TokenDeactivated>( @event => @event.OrderId ).To( saga => saga.Id );
            correlator.Correlate<CancelPrintJob>( command => command.AggregateId ).To( saga => saga.Id );
        }

        public Task Handle( Print command, IMessageContext context )
        {
            if ( Data.IdempotencyToken == command.IdempotencyToken )
            {
                return CompletedTask;
            }

            var orderId = command.AggregateId;
            var billingAccountId = command.BillingAccountId;
            var catalogId = command.CatalogId;
            var quantity = command.Quantity;
            var printJobQueued = new PrintJobQueued( orderId,
                                                     billingAccountId,
                                                     catalogId,
                                                     quantity,
                                                     command.CertificateThumbprint,
                                                     command.IdempotencyToken );

            Record( printJobQueued );

            var reservations = new List<ReserveToken>();

            for ( var i = 0; i < quantity; i++ )
            {
                reservations.Add( new ReserveToken( orderId, billingAccountId, catalogId ) );
            }

            return context.Send( reservations );
        }

        public Task Receive( TokenReserved @event, IMessageContext context )
        {
            var tokenId = @event.AggregateId;

            Record( new TokenSpooled( Id, tokenId, @event.Version, @event.Code, @event.Hash ) );

            if ( Data.SpooledTokens.Count == Data.QuantityRequested )
            {
                Record( new PrintJobSpooled( Id ) );
            }

            return CompletedTask;
        }

        public Task Handle( PrintToken command, IMessageContext context )
        {
            var tokenId = command.TokenId;

            if ( !Data.PrintedTokens.Contains( tokenId ) )
            {
                Record( new TokenPrinted( Id, tokenId ) );
            }

            if ( Data.PrintedTokens.Count == Data.QuantityRequested )
            {
                Record( new PrintJobCompleted( Id ) );
                MarkAsComplete();
            }

            return CompletedTask;
        }

        public Task Receive( TokenDeactivated @event, IMessageContext context )
        {
            var tokenId = @event.AggregateId;

            Record( new TokenUnspooled( Id, tokenId ) );

            if ( Data.SpooledTokens.Count == 0 )
            {
                MarkAsComplete();
            }

            return CompletedTask;
        }

        public Task Handle( CancelPrintJob command, IMessageContext context )
        {
            switch ( Data.State )
            {
                case Printing:
                    throw new PrintJobCancellationException( "Cannot cancel in the middle of printing." );
                case Printed:
                    throw new PrintJobAlreadyCompletedException();
                case Canceled:
                    return CompletedTask;
            }

            Record( new PrintJobCanceled( Id ) );

            if ( Data.SpooledTokens.Count == 0 )
            {
                MarkAsComplete();
                return CompletedTask;
            }

            var deactivatations = Data.SpooledTokens.Select( token => new DeactivateToken( token.Id, token.Version, Data.Id ) );
            return context.Send( deactivatations );
        }

        void Apply( PrintJobQueued @event )
        {
            Data.Id = @event.AggregateId;
            Data.State = Queued;
            Data.BillingAccountId = @event.BillingAccountId;
            Data.CatalogId = @event.CatalogId;
            Data.QuantityRequested = @event.Quantity;
            Data.CertificateThumbprint = @event.CertificateThumbprint;
            Data.IdempotencyToken = @event.IdempotencyToken;
        }

        void Apply( TokenSpooled @event )
        {
            var token = new TokenReference( @event.TokenId, @event.TokenVersion, @event.TokenCode, @event.TokenHash );

            Data.SpooledTokens.AddOrUpdate( token );
        }

        void Apply( TokenUnspooled @event ) => Data.SpooledTokens.Remove( @event.TokenId );

        void Apply( TokenPrinted @event )
        {
            Data.State = Printing;
            Data.PrintedTokens.Add( @event.TokenId );
        }

        void Apply( PrintJobSpooled @event ) => Data.State = ReadyForDownload;

        void Apply( PrintJobCompleted @event ) => Data.State = Printed;

        void Apply( PrintJobCanceled @event ) => Data.State = Canceled;
    }
}