namespace Contoso.Services.Filters
{
    using Microsoft.OData.Core;
    using System.Net;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using System.Web.Http.Results;

    static class HttpActionContextExtensions
    {
        internal static IHttpActionResult BadRequest( this HttpActionExecutedContext actionExecutedContext, string errorCode, string message ) =>
            actionExecutedContext.ActionContext.StatusCode( HttpStatusCode.BadRequest, errorCode, message );

        internal static IHttpActionResult BadRequest( this HttpActionContext actionContext, string errorCode, string message ) =>
            actionContext.StatusCode( HttpStatusCode.BadRequest, errorCode, message );

        internal static IHttpActionResult Conflict( this HttpActionExecutedContext actionExecutedContext, string errorCode, string message ) =>
            actionExecutedContext.ActionContext.StatusCode( HttpStatusCode.Conflict, errorCode, message );

        static IHttpActionResult StatusCode( this HttpActionContext actionContext, HttpStatusCode statusCode, string errorCode, string message )
        {
            var error = new ODataError() { ErrorCode = errorCode, Message = message };
            var configuration = actionContext.ControllerContext.Configuration;
            var contentNegotiator = configuration.Services.GetContentNegotiator();
            var formatters = configuration.Formatters;

            return new NegotiatedContentResult<ODataError>( statusCode, error, contentNegotiator, actionContext.Request, formatters );
        }
    }
}