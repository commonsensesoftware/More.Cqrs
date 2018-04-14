namespace Contoso.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;

    static class HttpContentExtensions
    {
        static HttpContentExtensions()
        {
            MediaTypeFormatters = new MediaTypeFormatterCollection();
            MediaTypeFormatters.JsonFormatter.SerializerSettings.ContractResolver = new ODataContractResolver();
        }

        static MediaTypeFormatterCollection MediaTypeFormatters { get; }

        internal static Task<T> ReadAsExampleAsync<T>( this HttpContent content, T example ) =>
            content.ReadAsAsync<T>( MediaTypeFormatters );
    }
}