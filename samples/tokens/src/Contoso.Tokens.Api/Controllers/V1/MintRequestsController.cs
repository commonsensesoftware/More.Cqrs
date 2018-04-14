namespace Contoso.Services.Controllers.V1
{
    using Domain.Accounts;
    using Domain.Tokens.Minting;
    using Filters;
    using Microsoft.Web.Http;
    using More.ComponentModel;
    using More.Domain.Commands;
    using System;
    using System.Collections;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;
    using MintRequest = Tokens.MintRequest;

    [ApiVersion( "1.0" )]
    [ODataRoutePrefix( "MintRequests" )]
    public class MintRequestsController : ControllerBase
    {
        readonly ICommandSender bus;
        readonly IReadOnlyRepository<MintRequest> repository;
        readonly IFindAccount accounts;

        public MintRequestsController( ICommandSender bus, IReadOnlyRepository<MintRequest> repository, IFindAccount accounts )
        {
            this.bus = bus;
            this.repository = repository;
            this.accounts = accounts;
        }

        [ODataRoute]
        public async Task<IHttpActionResult> Get( ODataQueryOptions<MintRequest> options, CancellationToken cancellationToken )
        {
            var querySettings = new ODataQuerySettings();

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var mintRequests = await repository.GetAsync( query => options.ApplyTo( query, querySettings ), circuitBreaker.Token );
                return this.Success( mintRequests );
            }
        }

        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Get( [FromODataUri] Guid id, ODataQueryOptions<MintRequest> options, CancellationToken cancellationToken )
        {
            var querySettings = new ODataQuerySettings();

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var orders = await repository.GetAsync( query => options.ApplyTo( query.Where( t => t.Id == id ), querySettings ), circuitBreaker.Token );
                var mintRequest = orders.SingleOrDefault();
                return this.SuccessOrNotFound( mintRequest );
            }
        }

        [ValidateNewMintRequest]
        [ODataRoute]
        public async Task<IHttpActionResult> Post(
            [FromBody] MintRequest newRequest,
            [FromHeader] Guid clientRequestId,
            [RequestChecksum] string idempotencyToken,
            ODataQueryOptions<MintRequest> options,
            CancellationToken cancellationToken )
        {
            var builder = new MintBuilder().ForCatalogItem( newRequest.CatalogId )
                                           .WithQuantity( newRequest.Quantity )
                                           .CorrelatedBy( clientRequestId.ToString() )
                                           .HasIdempotencyToken( idempotencyToken );

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                builder.ForBillingAccount( await accounts.FindBusinessAccount( User, circuitBreaker.Token ) );

                var mint = builder.NewMint();

                await bus.Send( mint, circuitBreaker.Token );

                newRequest.Id = mint.AggregateId;

                return await circuitBreaker.CreatedOrAccepted( newRequest, ct => repository.GetSingleAsync( t => t.Id == newRequest.Id, ct ) );
            }
        }

        [IfMatchRequired]
        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Delete(
            [FromODataUri] Guid id,
            ODataQueryOptions<MintRequest> options,
            CancellationToken cancellationToken,
            [FromHeader] Guid? clientRequestId = null )
        {
            var expectedVersion = options.ETagValue( t => t.Version );
            var cancel = new CancelMint( id, expectedVersion ) { CorrelationId = clientRequestId?.ToString() };

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                await bus.Send( cancel, circuitBreaker.Token );
            }

            return this.NoContent();
        }
    }
}