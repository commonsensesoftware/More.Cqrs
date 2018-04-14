namespace Contoso.Services
{
    using Contoso.Domain.Tokens;
    using Contoso.Domain.Tokens.Minting;
    using Contoso.Domain.Tokens.Ordering;
    using Microsoft.OData.Core.UriParser;
    using Microsoft.OData.Core.UriParser.Semantic;
    using Microsoft.OData.Edm;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Web.OData.Builder;

    static class HttpResponseMessageExtensions
    {
        static readonly Uri LocalHost = new Uri( "http://localhost" );
        static readonly Lazy<IEdmModel> model = new Lazy<IEdmModel>( NewSimpleEdmModel );

        internal static TKey IdFromLocationHeader<TKey>( this HttpResponseMessage response )
        {
            var location = response.Headers.Location;

            if ( location == null )
            {
                return default( TKey );
            }

            var parser = new ODataUriParser( model.Value, LocalHost, location );
            var path = parser.ParsePath();
            var key = path.OfType<KeySegment>().Single().Keys.Single().Value;

            return (TKey) key;
        }

        static IEdmModel NewSimpleEdmModel()
        {
            var builder = new ODataModelBuilder();

            builder.EntitySet<MintRequest>( "MintRequests" ).EntityType.HasKey( mr => mr.Id );
            builder.EntitySet<Order>( "Orders" ).EntityType.HasKey( o => o.Id );
            builder.EntitySet<Token>( "Tokens" ).EntityType.HasKey( t => t.Id );

            return builder.GetEdmModel();
        }
    }
}