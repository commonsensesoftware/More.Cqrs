namespace Contoso.Services.Controllers
{
    using More.Domain;
    using System;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Metadata;
    using static System.AttributeTargets;

    [AttributeUsage( Parameter, Inherited = true, AllowMultiple = false )]
    public sealed class RequestChecksumAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding( HttpParameterDescriptor parameter )
        {
            Contract.Assume( parameter != null );

            if ( parameter.ParameterType == typeof( string ) )
            {
                return new ChecksumHttpParameterBinding( parameter );
            }

            return parameter.BindAsError( $"A checksum parameter must be of type string, but the parameter {parameter.ParameterName} is of type {parameter.ParameterType}." );
        }

        sealed class ChecksumHttpParameterBinding : HttpParameterBinding
        {
            internal ChecksumHttpParameterBinding( HttpParameterDescriptor descriptor ) : base( descriptor ) { }

            public override async Task ExecuteBindingAsync( ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken )
            {
                Contract.Assume( metadataProvider != null );
                Contract.Assume( actionContext != null );

                var request = actionContext.Request;
                var content = request.Content;

                if ( content != null )
                {
                    await content.LoadIntoBufferAsync();
                }

                var message = new HttpMessageContent( request );

                using ( var stream = await message.ReadAsStreamAsync() )
                {
                    actionContext.ActionArguments[Descriptor.ParameterName] = Checksum.AsBase64( stream );
                }
            }
        }
    }
}