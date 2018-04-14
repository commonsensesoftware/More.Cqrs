namespace Contoso.Services.Controllers.V1
{
    using Contoso.Domain.Security;
    using Contoso.Domain.Tokens;
    using Contoso.Domain.Tokens.Printing;
    using Domain.Accounts;
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
    using static SR;
    using static System.TimeSpan;
    using static Tokens.PrintingState;
    using Order = Tokens.Order;
    using PrintJob = Tokens.PrintJob;

    [ApiVersion( "1.0" )]
    [ODataRoutePrefix( "PrintJobs" )]
    public class PrintJobsController : ControllerBase
    {
        readonly ICommandSender bus;
        readonly IReadOnlyRepository<PrintJob> repository;
        readonly IFindAccount accounts;
        readonly ICertificateLocator certificateLocator;
        readonly ITokenSecurity tokenSecurity;

        public PrintJobsController(
            ICommandSender bus,
            IReadOnlyRepository<PrintJob> repository,
            IFindAccount accounts,
            ICertificateLocator certificateLocator,
            ITokenSecurity tokenSecurity )
        {
            this.bus = bus;
            this.repository = repository;
            this.accounts = accounts;
            this.certificateLocator = certificateLocator;
            this.tokenSecurity = tokenSecurity;
        }

        [ODataRoute]
        public async Task<IHttpActionResult> Get( ODataQueryOptions<Order> options, CancellationToken cancellationToken )
        {
            var querySettings = new ODataQuerySettings();

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var printJobs = await repository.GetAsync( query => options.ApplyTo( query, querySettings ), circuitBreaker.Token );
                return this.Success( printJobs );
            }
        }

        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Get( [FromODataUri] Guid id, ODataQueryOptions<Order> options, CancellationToken cancellationToken )
        {
            var querySettings = new ODataQuerySettings();

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var printJobs = await repository.GetAsync( query => options.ApplyTo( query.Where( o => o.Id == id ), querySettings ), circuitBreaker.Token );
                var printJob = printJobs.SingleOrDefault();
                return this.SuccessOrNotFound( printJob );
            }
        }

        [HttpGet]
        [ODataRoute( "({id})/$value" )]
        public async Task<IHttpActionResult> GetValue( [FromODataUri] Guid id, CancellationToken cancellationToken )
        {
            var printJob = await repository.GetSingleAsync( r => r.Id == id );

            if ( printJob == null )
            {
                return NotFound();
            }

            if ( printJob.State != ReadyForDownload )
            {
                return this.Forbidden( nameof( TokensNotReadyForDownload ), TokensNotReadyForDownload );
            }

            var packageGenerator = new SecureTokenPackageGenerator( certificateLocator, tokenSecurity );
            var tokens = printJob.Tokens.Select( t => new TokenReference( t.Id, t.Version, t.Code, t.Hash ) );
            var package = await packageGenerator.CreatePackage( tokens, printJob.CertificateThumbprint );

            return this.SuccessOrPartialContent( package, package.MediaType );
        }

        [HttpHead]
        [ODataRoute( "({id})/$value" )]
        public async Task<IHttpActionResult> HeadValue( [FromODataUri] Guid id, CancellationToken cancellationToken )
        {
            var printJob = await repository.GetSingleAsync( r => r.Id == id );

            if ( printJob == null )
            {
                return NotFound();
            }

            if ( printJob.State != ReadyForDownload )
            {
                return this.Forbidden( nameof( TokensNotReadyForDownload ), TokensNotReadyForDownload );
            }

            var packageGenerator = new SecureTokenPackageGenerator( certificateLocator, tokenSecurity );
            var tokens = printJob.Tokens.Select( t => new TokenReference( t.Id, t.Version, t.Code, t.Hash ) );

            using ( var package = await packageGenerator.CreatePackage( tokens, printJob.CertificateThumbprint ) )
            {
                return this.OkWithContentHeaders( package.Length, package.MediaType );
            }
        }

        [ValidateNewPrintJob]
        [ODataRoute]
        public async Task<IHttpActionResult> Post(
            [FromBody] PrintJob newPrintJob,
            [FromHeader] Guid clientRequestId,
            [RequestChecksum] string idempotencyToken,
            ODataQueryOptions<PrintJob> options,
            CancellationToken cancellationToken )
        {
            var thumbprint = certificateLocator.LocateByAccount( User ).Thumbprint;
            var builder = new PrintJobBuilder().ForCatalogItem( newPrintJob.CatalogId )
                                               .WithQuantity( newPrintJob.Quantity )
                                               .WithCertificateThumbprint( thumbprint )
                                               .CorrelatedBy( clientRequestId.ToString() )
                                               .HasIdempotencyToken( idempotencyToken );

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                builder.ForBillingAccount( await accounts.FindBusinessAccount( User, circuitBreaker.Token ) );

                var print = builder.NewPrint();

                await bus.Send( print, circuitBreaker.Token );

                newPrintJob.Id = print.AggregateId;
                newPrintJob.BillingAccountId = print.BillingAccountId;

                return await circuitBreaker.CreatedOrAccepted( newPrintJob, ct => repository.GetSingleAsync( t => t.Id == newPrintJob.Id, ct ) );
            }
        }

        // TODO: add cancellation validation
        [IfMatchRequired]
        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Delete(
            [FromODataUri] Guid id,
            ODataQueryOptions<Order> options,
            CancellationToken cancellationToken,
            [FromHeader] Guid? clientRequestId = null )
        {
            var expectedVersion = options.ETagValue( t => t.Version );
            var cancel = new CancelPrintJob( id, expectedVersion ) { CorrelationId = clientRequestId?.ToString() };

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                await bus.Send( cancel, circuitBreaker.Token );
            }

            return this.NoContent();
        }
    }
}