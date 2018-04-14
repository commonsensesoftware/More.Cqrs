namespace Contoso.Services
{
    using Microsoft.Owin.Testing;
    using Microsoft.Web.Http;
    using More.Domain.Messaging;
    using More.Domain.Sagas;
    using Owin;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static System.Guid;
    using static System.Net.Http.HttpMethod;
    using static System.String;
    using static System.StringComparison;

    [Trait( "Kind", "Acceptance" )]
    public abstract class ApiTest : IDisposable
    {
        const string JsonMediaType = "application/json";
        static readonly HttpMethod Patch = new HttpMethod( "PATCH" );
        readonly ITestOutputHelper output;
        bool disposed;

        protected ApiTest( ITestOutputHelper output )
        {
            var startup = new TestStartup( output );

            this.output = output;
            Server = TestServer.Create( startup.Configuration );
            Client = Server.HttpClient;
            Client.BaseAddress = new Uri( "http://localhost" );
            Client.DefaultRequestHeaders.Add( "Host", "localhost" );
        }

        protected TestServer Server { get; }

        protected HttpClient Client { get; }

        protected abstract ApiVersion ApiVersion { get; }

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            Client.Dispose();
            Server.Dispose();
        }

        Uri MakeAbsoluteUrl( string requestUri )
        {
            var url = new Uri( Client.BaseAddress, requestUri );
            var urlBuilder = new UriBuilder( url );
            var query = urlBuilder.Query;

            if ( !IsNullOrEmpty( query ) && !query.Contains( "api-version", OrdinalIgnoreCase ) )
            {
                query = query.TrimStart( '?' );

                if ( query.Length > 1 )
                {
                    query += "&";
                }
            }

            query += "api-version=" + ApiVersion.ToString();
            urlBuilder.Query = query;

            return urlBuilder.Uri;
        }

        HttpRequestMessage CreateRequest<TEntity>( string requestUri, TEntity entity, HttpMethod method )
        {
            var url = MakeAbsoluteUrl( requestUri );
            var request = new HttpRequestMessage( method, url );
            var json = new MediaTypeWithQualityHeaderValue( JsonMediaType );
            var accept = Client.DefaultRequestHeaders.Accept;

            if ( !Equals( entity, default( TEntity ) ) )
            {
                var formatter = new JsonMediaTypeFormatter();
                request.Content = new ObjectContentWithLength<TEntity>( entity, formatter, JsonMediaType );
            }

            if ( !accept.Contains( json ) )
            {
                accept.Add( json );
            }

            return request;
        }

        public void Accept( string metadata = null )
        {
            var mediaType = new MediaTypeWithQualityHeaderValue( JsonMediaType );
            var odataMetadata = new NameValueHeaderValue( "odata.metadata" );

            if ( IsNullOrEmpty( metadata ) )
            {
                odataMetadata.Value = "none";
            }
            else
            {
                switch ( metadata.ToUpperInvariant() )
                {
                    case "NONE":
                    case "MINIMAL":
                    case "FULL":
                        break;
                    default:
                        throw new ArgumentOutOfRangeException( nameof( metadata ), "The specified metadata value must be 'none', 'minimal', or 'full'." );
                }

                odataMetadata.Value = metadata;
            }

            mediaType.Parameters.Add( odataMetadata );
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add( mediaType );
        }

        public void PreferMinimalReturn() => Client.DefaultRequestHeaders.Add( "Prefer", "return=minimal" );

        public void PreferReturnEntity() => Client.DefaultRequestHeaders.Add( "Prefer", "return=representation" );

        public void AddHeader( string name, string value, bool allowMultiple = false )
        {
            var headers = Client.DefaultRequestHeaders;

            if ( allowMultiple || !headers.Contains( name ) )
            {
                headers.TryAddWithoutValidation( name, value );
            }
        }

        public Guid AddClientRequestId()
        {
            var clientRequestId = NewGuid();
            AddClientRequestId( clientRequestId );
            return clientRequestId;
        }

        public void AddClientRequestId( Guid clientRequestId ) => Client.DefaultRequestHeaders.Add( "client-request-id", clientRequestId.ToString() );

        public virtual Task<HttpResponseMessage> GetAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Get ) );

        public virtual Task<HttpResponseMessage> PostAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Post ) );

        public virtual Task<HttpResponseMessage> PostAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Post ) );

        public virtual Task<HttpResponseMessage> PutAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Put ) );

        public virtual Task<HttpResponseMessage> PatchAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Patch ) );

        public virtual Task<HttpResponseMessage> DeleteAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Delete ) );

        protected SagaFinder<TData> Saga<TData>() where TData : class, ISagaData => new SagaFinder<TData>();

        protected DefinedAggregates Aggregate { get; } = new DefinedAggregates();

        protected void WriteLine( string message ) => output.WriteLine( message );

        protected void WriteLine( string format, params object[] args ) => output.WriteLine( format, args );

        sealed class TestStartup : Startup
        {
            readonly ITestOutputHelper output;

            internal TestStartup( ITestOutputHelper output ) => this.output = output;

            public override void Configuration( IAppBuilder app )
            {
                var observers = new IObserver<IMessageDescriptor>[] { new MessageObserver( output ) };
                app.Properties.Add( "bus.Observers", observers );
                base.Configuration( app );
            }
        }

        sealed class MessageObserver : IObserver<IMessageDescriptor>
        {
            readonly ITestOutputHelper output;

            internal MessageObserver( ITestOutputHelper output ) => this.output = output;

            public void OnCompleted() { }

            public void OnError( Exception error ) => output.WriteLine( error.ToString() );

            public void OnNext( IMessageDescriptor value ) =>
                output.WriteLine( $"{value.Message.GetType().Name}, Id = {value.MessageId}, Correlation = {value.Message.CorrelationId}" );
        }

        sealed class ObjectContentWithLength<T> : ObjectContent<T>
        {
            long computedLength = -1L;

            internal ObjectContentWithLength( T value, MediaTypeFormatter formatter, string mediaType ) : base( value, formatter, mediaType ) { }

            protected override async Task SerializeToStreamAsync( Stream stream, TransportContext context )
            {
                computedLength = -1L;
                await base.SerializeToStreamAsync( stream, context );
                computedLength = stream.Length;
            }

            protected override bool TryComputeLength( out long length ) => ( length = computedLength ) > -1L;
        }
    }
}