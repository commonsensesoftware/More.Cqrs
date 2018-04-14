namespace Contoso.Services
{
    using Microsoft.OData.Core;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using static System.String;

    sealed class ODataRequestAdapter : IODataRequestMessage
    {
        readonly HttpRequestMessage request;

        internal ODataRequestAdapter( HttpRequestMessage request ) => this.request = request;

        public IEnumerable<KeyValuePair<string, string>> Headers =>
            request.Headers.Select( header => new KeyValuePair<string, string>( header.Key, Join( ",", header.Value ) ) );

        public string Method
        {
            get => request.Method.Method;
            set => throw new NotSupportedException();
        }

        public Uri Url
        {
            get => request.RequestUri;
            set => throw new NotSupportedException();
        }

        public string GetHeader( string headerName )
        {
            if ( request.Headers.TryGetValues( headerName, out var values ) )
            {
                return Join( ",", values );
            }

            return null;
        }

        public Stream GetStream() => throw new NotSupportedException();

        public void SetHeader( string headerName, string headerValue ) => request.Headers.TryAddWithoutValidation( headerName, headerValue );
    }
}