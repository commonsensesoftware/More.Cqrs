namespace Contoso.Services.Filters
{
    using Controllers;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using System.Web.OData;
    using System.Web.OData.Extensions;
    using System.Web.OData.Query;
    using Tokens;
    using static SR;
    using static System.AttributeTargets;

    [AttributeUsage( Method, Inherited = true, AllowMultiple = false )]
    public sealed class ValidateNewMintRequestAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutingAsync( HttpActionContext actionContext, CancellationToken cancellationToken )
        {
            var odata = actionContext.Request.ODataProperties();
            var context = new ODataQueryContext( odata.Model, typeof( Order ), odata.Path );
            var options = new ODataQueryOptions<Order>( context, actionContext.Request );
            var modelState = actionContext.ModelState;
            var result = default( IHttpActionResult );

            if ( !options.IsValidField( o => o.CatalogId, modelState ) )
            {
                result = actionContext.BadRequest( nameof( InvalidMintItem ), InvalidMintItem );
            }
            else if ( !options.IsValidField( o => o.Quantity, modelState ) )
            {
                result = actionContext.BadRequest( nameof( InvalidMintQuantity ), InvalidMintQuantity );
            }
            else
            {
                return;
            }

            actionContext.Response = await result.ExecuteAsync( cancellationToken );
        }
    }
}