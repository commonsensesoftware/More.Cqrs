namespace Contoso.Services
{
    using System;
    using System.Net.Http;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class LogicalHttpRequestMessage : DelegatingHandler
    {
        const string SlotKey = "MS_" + nameof( LogicalHttpRequestMessage );

        public static HttpRequestMessage Current
        {
            get
            {
                return (HttpRequestMessage) CallContext.LogicalGetData( SlotKey );
            }
            private set
            {
                if ( value == null )
                {
                    CallContext.FreeNamedDataSlot( SlotKey );
                }
                else
                {
                    CallContext.LogicalSetData( SlotKey, value );
                }
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            Current = request;
            var response = await base.SendAsync( request, cancellationToken );
            Current = null;
            return response;
        }
    }
}