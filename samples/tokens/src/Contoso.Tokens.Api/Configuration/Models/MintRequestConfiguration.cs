namespace Contoso.Services.Configuration.Models
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using System.Web.OData.Builder;
    using Tokens;
    using static DefinedApiVersions;

    public class MintRequestConfiguration : IModelConfiguration
    {
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            if ( apiVersion == V1 )
            {
                ApplyForVersion1( builder );
            }
        }

        static void ApplyForVersion1( ODataModelBuilder builder )
        {
            var mintRequests = builder.EntitySet<MintRequest>( "MintRequests" );
            var mintRequest = mintRequests.EntityType;

            mintRequest.HasKey( mr => mr.Id );
            mintRequest.Property( mr => mr.Version ).IsConcurrencyToken();
        }
    }
}