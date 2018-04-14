namespace Contoso.Services.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Filters;
    using static SR;
    using static System.AttributeTargets;
    using Common = More.Domain;
    using Domain = Domain.Tokens;

    [AttributeUsage( Class | Method, Inherited = true, AllowMultiple = false )]
    public sealed class TokenExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly Dictionary<Type, Func<HttpActionExecutedContext, IHttpActionResult>> errorResults =
            new Dictionary<Type, Func<HttpActionExecutedContext, IHttpActionResult>>()
            {
                [typeof( Domain.TokenNotActivatedException )] = c => c.BadRequest( nameof( TokenNotActivated ), TokenNotActivated ),
                [typeof( Domain.TokenAlreadyActivatedException )] = c => c.Conflict( nameof( TokenAlreadyActivated ), TokenAlreadyActivated ),
                [typeof( Domain.TokenAlreadyReservedException )] = c => c.Conflict( nameof( TokenReserved ), TokenReserved ),
                [typeof( Domain.TokenAlreadyRedeemedException )] = c => c.Conflict( nameof( TokenAlreadyRedeemed ), TokenAlreadyRedeemed ),
                [typeof( Domain.TokenVoidedException )] = c => c.Conflict( nameof( TokenVoided ), TokenVoided ),
                [typeof( Common.ConcurrencyException )] = c => c.Conflict( nameof( TokenModified ), TokenModified )
            };

        public override async Task OnExceptionAsync( HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken )
        {
            var key = actionExecutedContext.Exception.GetType();

            if ( !errorResults.TryGetValue( key, out var createResult ) )
            {
                return;
            }

            var result = createResult( actionExecutedContext );

            actionExecutedContext.Response = await result.ExecuteAsync( cancellationToken );
        }
    }
}