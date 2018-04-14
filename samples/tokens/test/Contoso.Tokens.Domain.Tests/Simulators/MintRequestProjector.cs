namespace Contoso.Domain.Simulators
{
    using Contoso.Domain.Tokens.Minting;
    using More.Domain;
    using More.Domain.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static Contoso.Tokens.MintingState;
    using static System.Threading.Tasks.Task;
    using MintingState = Contoso.Tokens.MintingState;
    using MintRequest = Contoso.Tokens.MintRequest;

    class MintRequestProjector :
        IReceiveEvent<MintRequested>,
        IReceiveEvent<MintCanceling>,
        IReceiveEvent<MintCanceled>,
        IReceiveEvent<Minted>
    {
        readonly List<MintRequest> mintRequests = new List<MintRequest>();

        public IReadOnlyList<MintRequest> MintRequests => mintRequests;

        public Task Receive( Minted @event, IMessageContext context )
        {
            TransitionState( @event.AggregateId, Completed );
            return CompletedTask;
        }

        public Task Receive( MintCanceled @event, IMessageContext context )
        {
            TransitionState( @event.AggregateId, Canceled );
            return CompletedTask;
        }

        public Task Receive( MintCanceling @event, IMessageContext context )
        {
            TransitionState( @event.AggregateId, Canceling );
            return CompletedTask;
        }

        public Task Receive( MintRequested @event, IMessageContext context )
        {
            var mintRequest = mintRequests.SingleOrDefault( mr => mr.Id == @event.AggregateId );

            if ( mintRequest == null )
            {
                mintRequests.Add(
                    new MintRequest()
                    {
                        Id = @event.AggregateId,
                        Version = @event.Version,
                        CatalogId = @event.CatalogId,
                        Quantity = @event.MintJobs.Sum( mj => mj.Count ),
                        State = Started
                    } );
            }

            return CompletedTask;
        }

        void TransitionState( Guid id, MintingState state )
        {
            var mintRequest = mintRequests.SingleOrDefault( mr => mr.Id == id );

            if ( mintRequest != null )
            {
                mintRequest.State = state;
            }
        }
    }
}