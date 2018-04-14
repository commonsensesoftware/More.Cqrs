namespace Contoso.Services.Filters
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using static System.AttributeTargets;

    [AttributeUsage( Class | Method, Inherited = true, AllowMultiple = false )]
    public sealed class IfMatchRequiredAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutingAsync( HttpActionContext actionContext, CancellationToken cancellationToken )
        {
            var request = actionContext.Request;
            var ifMatch = request.Headers.IfMatch;

            if ( ifMatch.Count == 1 )
            {
                return;
            }

            var result = actionContext.BadRequest( "InvalidEntityTag", "The request requires exactly one entity tag value provided in the If-Match header." );

            actionContext.Response = await result.ExecuteAsync( cancellationToken );
        }
    }
}