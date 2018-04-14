namespace Contoso.Services.Controllers
{
    using Microsoft.OData.Core;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData.Results;
    using static System.TimeSpan;

    public class CircuitBreaker<TController> : CancellationTokenSource where TController : ApiController, IHaveServiceLevelAgreement
    {
        readonly TController controller;

        public CircuitBreaker( TController controller, CancellationToken cancellationToken ) : base( controller.Timeout )
        {
            this.controller = controller;
            cancellationToken.Register( Cancel );
        }

        public Task<IHttpActionResult> CreatedOrAccepted<TResource>( TResource resource, Func<CancellationToken, Task<TResource>> queryResource )
            where TResource : class => CreatedOrAccepted( resource, queryResource, retryAfter: FromSeconds( 2d ) );

        public async Task<IHttpActionResult> CreatedOrAccepted<TResource>(
            TResource resource,
            Func<CancellationToken, Task<TResource>> queryResource,
            TimeSpan retryAfter ) where TResource : class
        {
            var prefer = controller.Request.PreferHeader();

            if ( prefer.RespondAsync )
            {
                return Accepted( resource, retryAfter );
            }

            if ( prefer.ReturnContent == true )
            {
                var holder = new StrongBox<TResource>();

                if ( !await QueryResourceBeforeInterrupted( queryResource, holder ).ConfigureAwait( false ) )
                {
                    return Accepted( resource, retryAfter );
                }

                resource = holder.Value;
            }

            return new CreatedODataResult<TResource>( resource, controller );
        }

        async Task<bool> QueryResourceBeforeInterrupted<TResource>( Func<CancellationToken, Task<TResource>> queryResource, StrongBox<TResource> holder )
        {
            const int OneTenthOfASecond = 100;
            var resource = default( TResource );

            try
            {
                resource = await queryResource( Token );

                while ( resource == null )
                {
                    await Task.Delay( OneTenthOfASecond ).ConfigureAwait( false );
                    resource = await queryResource( Token ).ConfigureAwait( false );
                }
            }
            catch ( OperationCanceledException )
            {
                return false;
            }

            holder.Value = resource;
            return true;
        }

        IHttpActionResult Accepted<TResource>( TResource resource, TimeSpan retryAfter ) =>
            new AcceptedResult( new CreatedODataResult<TResource>( resource, controller ), retryAfter );

        sealed class AcceptedResult : IHttpActionResult
        {
            readonly IHttpActionResult innerResult;
            readonly TimeSpan retryAfter;

            internal AcceptedResult( IHttpActionResult innerResult, TimeSpan retryAfter )
            {
                this.innerResult = innerResult;
                this.retryAfter = retryAfter;
            }

            public async Task<HttpResponseMessage> ExecuteAsync( CancellationToken cancellationToken )
            {
                var response = await innerResult.ExecuteAsync( cancellationToken );

                response.Headers.RetryAfter = new RetryConditionHeaderValue( retryAfter );
                response.StatusCode = HttpStatusCode.Accepted;

                return response;
            }
        }
    }
}