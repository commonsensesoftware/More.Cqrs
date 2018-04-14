namespace Contoso.Services.Configuration.Models
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using System.Web.OData.Builder;
    using Tokens;
    using static DefinedApiVersions;

    public class OrderConfiguration : IModelConfiguration
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
            var orders = builder.EntitySet<Order>( "Orders" );
            var order = orders.EntityType;

            order.HasKey( o => o.Id );
            order.Property( o => o.Version ).IsConcurrencyToken();
            order.HasMany( o => o.Tokens );
        }
    }
}