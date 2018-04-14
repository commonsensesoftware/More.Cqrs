namespace Contoso.Services.Controllers.V1
{
    using Domain.Accounts;
    using Domain.Tokens;
    using Filters;
    using Microsoft.Web.Http;
    using More.ComponentModel;
    using More.Domain.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;
    using Token = Tokens.Token;

    [ApiVersion( "1.0" )]
    [ODataRoutePrefix( "Tokens" )]
    [TokenExceptionFilter]
    public class TokensController : ControllerBase
    {
        readonly ICommandSender bus;
        readonly IReadOnlyRepository<Token> repository;
        readonly ITokenSecurity tokenSecurity;
        readonly IFindAccount accounts;

        public TokensController(
            ICommandSender bus,
            IReadOnlyRepository<Token> repository,
            ITokenSecurity tokenSecurity,
            IFindAccount accounts )
        {
            this.bus = bus;
            this.repository = repository;
            this.tokenSecurity = tokenSecurity;
            this.accounts = accounts;
        }

        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Get( [FromODataUri] string id, ODataQueryOptions<Token> options, CancellationToken cancellationToken )
        {
            var querySettings = new ODataQuerySettings();

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var tokens = await repository.GetAsync( query => options.ApplyTo( query.Where( t => t.Id == id ), querySettings ), circuitBreaker.Token );
                var token = tokens.SingleOrDefault();
                return this.SuccessOrNotFound( token );
            }
        }

        [IfMatchRequired]
        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Delete(
            [FromODataUri] string id,
            ODataQueryOptions<Token> options,
            CancellationToken cancellationToken,
            [FromHeader] Guid? clientRequestId = null )
        {
            var expectedVersion = options.ETagValue( t => t.Version );
            var @void = new VoidToken( id, expectedVersion ) { CorrelationId = clientRequestId?.ToString() };

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                await bus.Send( @void, circuitBreaker.Token );
            }

            return this.NoContent();
        }

        [HttpPost]
        [IfMatchRequired]
        [ODataRoute( "({id})/Activate" )]
        public async Task<IHttpActionResult> Activate(
            [FromODataUri] string id,
            ODataQueryOptions<Token> options,
            CancellationToken cancellationToken,
            [FromHeader] Guid? clientRequestId = null )
        {
            var expectedVersion = options.ETagValue( t => t.Version );

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var billingAccountId = await accounts.FindBusinessAccount( User, circuitBreaker.Token );
                var activate = new ActivateToken( id, expectedVersion, billingAccountId ) { CorrelationId = clientRequestId?.ToString() };

                await bus.Send( activate, circuitBreaker.Token );
            }

            return Ok();
        }

        [HttpPost]
        [IfMatchRequired]
        [ODataRoute( "({id})/Deactivate" )]
        public async Task<IHttpActionResult> Deactivate(
            [FromODataUri] string id,
            ODataQueryOptions<Token> options,
            CancellationToken cancellationToken,
            [FromHeader] Guid? clientRequestId = null )
        {
            var expectedVersion = options.ETagValue( t => t.Version );
            var deactivate = new DeactivateToken( id, expectedVersion ) { CorrelationId = clientRequestId?.ToString() };

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                await bus.Send( deactivate, circuitBreaker.Token );
            }

            return Ok();
        }

        [HttpPost]
        [IfMatchRequired]
        [ODataRoute( "Redeem" )]
        public async Task<IHttpActionResult> Redeem(
            ODataActionParameters parameters,
            ODataQueryOptions<Token> options,
            CancellationToken cancellationToken,
            [FromHeader] Guid? clientRequestId = null )
        {
            var expectedVersion = options.ETagValue( t => t.Version );
            var fiveByFive = (string) parameters["code"];
            var tokenHash = HashFiveByFiveWithoutFormatting( fiveByFive );

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var token = await repository.GetSingleAsync( t => t.Hash == tokenHash, circuitBreaker.Token );
                var accountId = await accounts.FindConsumerAccount( User, circuitBreaker.Token );
                var redeem = new RedeemToken( token.Id, expectedVersion, accountId ) { CorrelationId = clientRequestId?.ToString() };

                await bus.Send( redeem, circuitBreaker.Token );
            }

            return Ok();
        }

        string HashFiveByFiveWithoutFormatting( string fiveByFive )
        {
            using ( var tokenCode = new System.Security.SecureString() )
            {
                fiveByFive.Replace( "-", string.Empty ).ForEach( tokenCode.AppendChar );
                tokenCode.MakeReadOnly();
                return tokenSecurity.HashAsBase64( tokenCode );
            }
        }
    }
}