namespace Contoso.Services
{
    using Microsoft.OData.Core;
    using System;
    using System.Net.Http;

    public static class HttpRequestMessageExtensions
    {
        public static ODataPreferenceHeader PreferHeader( this HttpRequestMessage request ) => new ODataRequestAdapter( request ).PreferHeader();
    }
}