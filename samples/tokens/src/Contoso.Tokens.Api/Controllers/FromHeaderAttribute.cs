namespace Contoso.Services.Controllers
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.ValueProviders;
    using static System.AttributeTargets;
    using static System.Char;
    using static System.Globalization.CultureInfo;
    using static System.Net.HttpStatusCode;
    using static System.String;

    [AttributeUsage( Parameter, Inherited = true, AllowMultiple = false )]
    public sealed class FromHeaderAttribute : ParameterBindingAttribute
    {
        public FromHeaderAttribute() { }

        public FromHeaderAttribute( string name ) => Name = name;

        public string Name { get; set; }

        public override HttpParameterBinding GetBinding( HttpParameterDescriptor parameter )
        {
            Contract.Assume( parameter != null );

            var headerName = Name;

            if ( IsNullOrEmpty( headerName ) )
            {
                headerName = HeaderValueProvider.HeaderNameByConvention( parameter.ParameterName );
            }

            return parameter.BindWithModelBinding( new HeaderValueProviderFactory( headerName, parameter.IsOptional ) );
        }

        sealed class HeaderValueProviderFactory : ValueProviderFactory
        {
            readonly string headerName;
            readonly bool optional;

            internal HeaderValueProviderFactory( string headerName, bool optional )
            {
                this.headerName = headerName;
                this.optional = optional;
            }

            public override IValueProvider GetValueProvider( HttpActionContext actionContext ) =>
                new HeaderValueProvider( headerName, optional, actionContext.Request );
        }

        sealed class HeaderValueProvider : IValueProvider
        {
            readonly string configuredHeaderName;
            readonly bool optional;
            readonly HttpRequestMessage request;

            internal HeaderValueProvider( string headerName, bool optional, HttpRequestMessage request )
            {
                configuredHeaderName = headerName;
                this.optional = optional;
                this.request = request;
            }

            public bool ContainsPrefix( string prefix ) => true;

            public ValueProviderResult GetValue( string key )
            {
                Contract.Assume( key != null );

                var headerName = configuredHeaderName ?? HeaderNameByConvention( RemovePrefix( key ) );

                if ( !request.Headers.TryGetValues( headerName, out var values ) )
                {
                    if ( optional )
                    {
                        return null;
                    }

                    var response = request.CreateErrorResponse( BadRequest, SR.MissingHeader.FormatDefault( headerName.ToLowerInvariant() ) );
                    throw new HttpResponseException( response );
                }

                var data = Join( ",", values );
                return new ValueProviderResult( values, data, InvariantCulture );
            }

            static string RemovePrefix( string key )
            {
                Contract.Requires( key != null );

                var index = key.LastIndexOf( '.' );
                return index < 0 ? key : key.Substring( index + 1 );
            }

            internal static string HeaderNameByConvention( string key )
            {
                Contract.Requires( key != null );

                if ( key.Length == 0 )
                {
                    return key;
                }

                var name = new StringBuilder();

                name.Append( key[0] );

                for ( var i = 1; i < key.Length; i++ )
                {
                    var c = key[i];

                    if ( IsUpper( c ) )
                    {
                        name.Append( '-' );
                    }

                    name.Append( c );
                }

                return name.ToString();
            }
        }
    }
}