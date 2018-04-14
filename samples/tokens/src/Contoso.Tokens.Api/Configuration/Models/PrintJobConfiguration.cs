namespace Contoso.Services.Configuration.Models
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using System.Web.OData.Builder;
    using Tokens;
    using static DefinedApiVersions;

    public class PrintJobConfiguration : IModelConfiguration
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
            var printJobs = builder.EntitySet<PrintJob>( "PrintJobs" );
            var printJob = printJobs.EntityType;

            printJob.HasKey( pj => pj.Id );
            printJob.Property( pj => pj.Version ).IsConcurrencyToken();
            printJob.HasMany( pj => pj.Tokens );
            // TODO: update this when the service can output encrypted and/or compressed content (*.zip)
            printJob.MediaType( "text/csv" );
            printJob.Ignore( jp => jp.CertificateThumbprint );
        }
    }
}