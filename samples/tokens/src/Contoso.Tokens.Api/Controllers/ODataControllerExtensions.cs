namespace Contoso.Services.Controllers
{
    using Microsoft.OData.Core;
    using System.Net;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Http.Results;
    using System.Web.OData;

    static class ODataControllerExtensions
    {
        internal static IHttpActionResult BadRequest( this ODataController controller, string errorCode, string message ) =>
            controller.BadRequest( new ODataError() { ErrorCode = errorCode, Message = message } );

        internal static IHttpActionResult BadRequest( this ODataController controller, ODataError error ) =>
            new NegotiatedContentResult<ODataError>( HttpStatusCode.BadRequest, error, controller );

        internal static IHttpActionResult Forbidden( this ODataController controller, string errorCode, string message ) =>
            controller.Forbidden( new ODataError() { ErrorCode = errorCode, Message = message } );

        internal static IHttpActionResult Forbidden( this ODataController controller, ODataError error ) =>
            new NegotiatedContentResult<ODataError>( HttpStatusCode.Forbidden, error, controller );

        internal static CircuitBreaker<TController> NewCircuitBreaker<TController>( this TController controller, CancellationToken cancellationToken )
            where TController : ApiController, IHaveServiceLevelAgreement => new CircuitBreaker<TController>( controller, cancellationToken );
    }
}