namespace Contoso.Domain.Tokens.Minting
{
    using More.Domain;
    using More.Domain.Commands;
    using More.Domain.Sagas;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using static MintingState;
    using static System.Math;
    using static System.Threading.Tasks.Task;

    public class MintRequest : Saga<MintRequestData>,
        IStartWith<Mint>,
        IHandleCommand<StartMintJob>,
        IHandleCommand<CancelMint>,
        IHandleCommand<StopMintJob>
    {
        const long MintBatchSize = 1000000L;

        protected override void CorrelateUsing( SagaCorrelator<MintRequestData> correlator )
        {
            correlator.Correlate<Mint>( command => command.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<StartMintJob>( command => command.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<CancelMint>( command => command.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<StopMintJob>( command => command.AggregateId ).To( saga => saga.Id );
        }

        public Task Handle( Mint command, IMessageContext context )
        {
            if ( Data.IdempotencyToken == command.IdempotencyToken )
            {
                return CompletedTask;
            }

            var @event = new MintRequested( command.AggregateId, command.CatalogId, command.IdempotencyToken );
            var commands = new List<StartMintJob>();
            var jobId = 1;
            var start = 1L;
            var remaining = command.Quantity;

            for ( ; start <= command.Quantity; start += MintBatchSize, remaining -= MintBatchSize, jobId++ )
            {
                var count = Min( remaining, MintBatchSize );
                var mintJob = new MintJob( jobId, command.CatalogId, start, count );
                var startMintJob = new StartMintJob( @event.AggregateId,
                                                     jobId - 1,
                                                     mintJob.Id,
                                                     mintJob.CatalogId,
                                                     mintJob.StartOfSequence,
                                                     mintJob.Count );

                @event.MintJobs.Add( mintJob );
                commands.Add( startMintJob );
            }

            Record( @event );
            return context.Send( commands );
        }

        public async Task Handle( StartMintJob command, IMessageContext context )
        {
            var jobId = command.JobId;

            if ( Data.State != Started || !Data.MintJobs.Contains( jobId ) )
            {
                return;
            }

            var tokenVault = (ITokenVault) context.GetService( typeof( ITokenVault ) );
            var tokenSecurity = (ITokenSecurity) context.GetService( typeof( ITokenSecurity ) );
            var die = new StandardTokenDie( command.AggregateId, command.StartOfSequence, tokenSecurity );
            var tokens = RunJob( jobId, die, context.CancellationToken );

            await tokenVault.Deposit( tokens, context.CancellationToken );

            Record( new MintJobFinished( Id, jobId ) );

            if ( Data.MintJobs.Count > 0 )
            {
                return;
            }

            if ( Data.PartiallyCanceled )
            {
                Record( new MintCanceled( Id ) );
            }
            else
            {
                await tokenVault.ReleaseToCirculation( Id, command.CorrelationId, context.CancellationToken );
                Record( new Minted( Id ) );
            }

            MarkAsComplete();
        }

        public Task Handle( CancelMint command, IMessageContext context )
        {
            if ( Data.State != Started )
            {
                return CompletedTask;
            }

            var mintJobIds = new List<int>();
            var commands = new List<StopMintJob>();

            foreach ( var mintJob in Data.MintJobs )
            {
                mintJobIds.Add( mintJob.Id );
                commands.Add( new StopMintJob( Id, mintJob.Id ) );
            }

            Record( new MintCanceling( Id, mintJobIds ) );
            return context.Send( commands );
        }

        public async Task Handle( StopMintJob command, IMessageContext context )
        {
            Record( new MintJobStopped( Id, command.JobId ) );

            if ( Data.MintJobs.Count > 0 )
            {
                return;
            }

            Record( new MintCanceled( Id ) );

            var tokenVault = (ITokenVault) context.GetService( typeof( ITokenVault ) );

            await tokenVault.RemoveFromCirculation( Id, command.CorrelationId, context.CancellationToken );
            MarkAsComplete();
        }

        IEnumerable<Token> RunJob( int jobId, ITokenDie die, CancellationToken cancellationToken )
        {
            var mintJob = Data.MintJobs[jobId];
            var sequenceNumber = mintJob.StartOfSequence - 1;
            var builder = new TokenBuilder().HasMintRequestId( Id ).HasCatalogId( Data.CatalogId );

            while ( sequenceNumber++ < mintJob.Count && !cancellationToken.IsCancellationRequested )
            {
                var planchet = new TokenPlanchet();

                die.Strike( planchet );
                builder.HasId( planchet.Id );
                builder.HasCode( planchet.Code );
                builder.HasHash( planchet.Hash );
                builder.HasSequenceNumber( sequenceNumber );

                yield return builder.CreateToken();
            }
        }

        void Apply( MintRequested @event )
        {
            Data.Id = @event.AggregateId;
            Data.State = Started;
            Data.CatalogId = @event.CatalogId;
            Data.IdempotencyToken = @event.IdempotencyToken;

            foreach ( var mintJob in @event.MintJobs )
            {
                Data.MintJobs.Add( mintJob );
            }
        }

        void Apply( Minted @event ) => Data.State = MintingState.Completed;

        void Apply( MintCanceling @event ) => Data.State = Canceling;

        void Apply( MintCanceled @event ) => Data.State = Canceled;

        void Apply( MintJobFinished @event ) => Data.MintJobs.Remove( @event.JobId );

        void Apply( MintJobStopped @event ) => Data.PartiallyCanceled |= Data.MintJobs.Remove( @event.JobId );
    }
}