namespace Contoso.Services.Controllers.V1
{
    using Contoso.Domain.Tokens;
    using Domain.Accounts;
    using Domain.Tokens.Ordering;
    using Filters;
    using Microsoft.OData.Core.UriParser.Semantic;
    using Microsoft.Web.Http;
    using More.ComponentModel;
    using More.Domain.Commands;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Dynamic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Results;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;
    using static System.Reflection.RuntimeReflectionExtensions;
    using static System.Runtime.InteropServices.Marshal;
    using static Tokens.OrderState;
    using Order = Tokens.Order;
    using SR = Services.SR;
    using Token = Tokens.Token;

    [ApiVersion( "1.0" )]
    [ODataRoutePrefix( "Orders" )]
    public class OrdersController : ControllerBase
    {
        readonly ICommandSender bus;
        readonly IReadOnlyRepository<Order> repository;
        readonly IFindAccount accounts;
        readonly ITokenSecurity tokenSecurity;

        public OrdersController( ICommandSender bus, IReadOnlyRepository<Order> repository, IFindAccount accounts, ITokenSecurity tokenSecurity )
        {
            this.bus = bus;
            this.repository = repository;
            this.accounts = accounts;
            this.tokenSecurity = tokenSecurity;
        }

        [ODataRoute]
        public async Task<IHttpActionResult> Get( ODataQueryOptions<Order> options, CancellationToken cancellationToken )
        {
            var querySettings = new ODataQuerySettings();

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var orders = await repository.GetAsync( query => options.ApplyTo( query, querySettings ), circuitBreaker.Token );
                return this.Success( orders );
            }
        }

        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Get( [FromODataUri] Guid id, ODataQueryOptions<Order> options, CancellationToken cancellationToken )
        {
            var querySettings = new ODataQuerySettings();

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                var orders = await repository.GetAsync( query => options.ApplyTo( query.Where( o => o.Id == id ), querySettings ), circuitBreaker.Token );
                var order = orders.SingleOrDefault();
                return this.SuccessOrNotFound( order );
            }
        }

        /// <summary>
        /// Returns the tokens associated with an order.
        /// </summary>
        /// <param name="id">The identifier of the order to retrieve the tokens for.</param>
        /// <param name="options">The <see cref="ODataQueryOptions{T}">OData query options</see> associated with the request.</param>
        /// <returns>A <seee cref="Task{TResult}">task</seee> containing the <see cref="IHttpActionResult">result</see>.</returns>
        /// <remarks>This is the only mechanism that allows a caller to retrieve decrypted token codes. Tokens are not accessible
        /// until the order has been fulfilled. This simplifies the order cancellation process.</remarks>
        [ODataRoute( "({id})/Tokens" )]
        public async Task<IHttpActionResult> GetTokens( [FromODataUri] Guid id, ODataQueryOptions<Token> options, CancellationToken cancellationToken )
        {
            if ( IsProjectionIncludingCodeButNotHash( options ) )
            {
                // note: this implementation requires the token hash to perform decryption.
                // we could look up the hash code for each result, but it would be slow.
                return this.BadRequest( "InvalidQueryOption", SR.TokenHashRequired );
            }

            var querySettings = new ODataQuerySettings();
            var results = default( IEnumerable );

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                results = await repository.SubqueryAsync( o => o.Id == id && o.State == Fulfilled, o => o.Tokens, options, circuitBreaker.Token );
            }

            return results == null ? NotFound() : OkWithDecryptedTokens( results );
        }

        [ValidateNewOrder]
        [ODataRoute]
        public async Task<IHttpActionResult> Post(
            [FromBody] Order newOrder,
            [FromHeader] Guid clientRequestId,
            [RequestChecksum] string idempotencyToken,
            ODataQueryOptions<Order> options,
            CancellationToken cancellationToken,
            bool reserveOnly = false )
        {
            var builder = new OrderBuilder().ForCatalogItem( newOrder.CatalogId )
                                            .WithQuantity( newOrder.Quantity )
                                            .CorrelatedBy( clientRequestId.ToString() )
                                            .HasIdempotencyToken( idempotencyToken );

            if ( reserveOnly )
            {
                builder.ReserveTokensOnly();
            }

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                builder.ForBillingAccount( await accounts.FindBusinessAccount( User, circuitBreaker.Token ) );

                var placeOrder = builder.NewPlaceOrder();

                await bus.Send( placeOrder, circuitBreaker.Token );

                newOrder.Id = placeOrder.AggregateId;
                newOrder.BillingAccountId = placeOrder.BillingAccountId;

                return await circuitBreaker.CreatedOrAccepted( newOrder, ct => repository.GetSingleAsync( t => t.Id == newOrder.Id, ct ) );
            }
        }

        [IfMatchRequired]
        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Delete(
            [FromODataUri] Guid id,
            ODataQueryOptions<Order> options,
            CancellationToken cancellationToken,
            [FromHeader] Guid? clientRequestId = null )
        {
            var expectedVersion = options.ETagValue( t => t.Version );
            var cancel = new CancelOrder( id, expectedVersion ) { CorrelationId = clientRequestId?.ToString() };

            using ( var circuitBreaker = this.NewCircuitBreaker( cancellationToken ) )
            {
                await bus.Send( cancel, circuitBreaker.Token );
            }

            return this.NoContent();
        }

        bool IsProjectionIncludingCodeButNotHash( ODataQueryOptions<Token> options )
        {
            Contract.Requires( options != null );

            var selectExpand = options.SelectExpand;

            if ( selectExpand == null )
            {
                return false;
            }

            var properties = selectExpand.SelectExpandClause.SelectedItems.OfType<PathSelectItem>().Select( i => i.SelectedPath.LastSegment ).OfType<PropertySegment>();
            var names = new HashSet<string>( properties.Select( p => p.Property.Name ) );
            var code = options.MapClrPropertyToEdmProperty( t => t.Code ).Name;
            var hash = options.MapClrPropertyToEdmProperty( t => t.Hash ).Name;

            return names.Contains( code ) && !names.Contains( hash );
        }

        IHttpActionResult OkWithDecryptedTokens( IEnumerable results )
        {
            Contract.Requires( results != null );
            Contract.Ensures( Contract.Result<IHttpActionResult>() != null );

            if ( results is IEnumerable<Token> typedResults )
            {
                return Ok( new DecryptedTokenEnumerator( typedResults, tokenSecurity ) );
            }

            // HACK: addresses known bug where the media type formatters do not correctly setup the response for an untyped object.
            // this behavior occurs when the returned object is the result of a query projection.
            var resultType = results.GetType();
            var elementType = resultType.GenericTypeArguments[0];
            var enumeratorType = typeof( DecryptedFiveByFiveEnumerator<> ).MakeGenericType( elementType );
            var decryptedResults = Activator.CreateInstance( enumeratorType, results, tokenSecurity );
            var response = Request.CreateResponse( HttpStatusCode.OK, enumeratorType, decryptedResults );

            return new ResponseMessageResult( response );
        }
    }

    /// <summary>
    ///  Provides a dynamic proxy that we can use to traverse PropertyContainer objects used in projections.
    ///  <seealso href="https://github.com/OData/WebApi/blob/master/OData/src/System.Web.OData/OData/Query/Expressions/PropertyContainer.cs"/>.
    /// </summary>
    /// <remarks>This is needed so that we can decrypt token codes within projection results.</remarks>
    sealed class ProjectionProxy : DynamicObject
    {
        readonly static ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> propertyMap =
            new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();
        readonly object instance;
        readonly Type instanceType;

        internal ProjectionProxy( object instance )
        {
            this.instance = instance;
            instanceType = instance.GetType();
        }

        public override bool TryGetMember( GetMemberBinder binder, out object result )
        {
            var properties = propertyMap.GetOrAdd( instanceType, type => type.GetRuntimeProperties().ToDictionary( p => p.Name ) );

            if ( properties.TryGetValue( binder.Name, out var property ) )
            {
                var value = property.GetValue( instance );
                result = value == null || IsPrimitive( value.GetType() ) ? value : new ProjectionProxy( value );
                return true;
            }
            else if ( binder.Name == "Next" )
            {
                result = null;
                return true;
            }

            return base.TryGetMember( binder, out result );
        }

        public override bool TrySetMember( SetMemberBinder binder, object value )
        {
            var properties = propertyMap.GetOrAdd( instanceType, type => type.GetRuntimeProperties().ToDictionary( p => p.Name ) );

            if ( properties.TryGetValue( binder.Name, out var property ) )
            {
                var proxy = value as ProjectionProxy;

                if ( proxy == null )
                {
                    property.SetValue( instance, value );
                }
                else
                {
                    property.SetValue( instance, proxy.instance );
                }

                return true;
            }

            return base.TrySetMember( binder, value );
        }

        static bool IsPrimitive( Type type )
        {
            var typeCode = Type.GetTypeCode( type );

            if ( typeCode > TypeCode.Object )
            {
                return true;
            }

            return typeof( Guid ).Equals( type ) ||
                   typeof( TimeSpan ).Equals( type ) ||
                   ( type.IsGenericType && typeof( Nullable<> ).Equals( type.GetGenericTypeDefinition() ) );
        }
    }

    sealed class DecryptedFiveByFiveEnumerator<T> : IEnumerable<T>
    {
        readonly IQueryable<T> encryptedQuery;
        readonly ITokenSecurity tokenSecurity;

        public DecryptedFiveByFiveEnumerator( IQueryable<T> encryptedQuery, ITokenSecurity tokenSecurity )
        {
            this.encryptedQuery = encryptedQuery;
            this.tokenSecurity = tokenSecurity;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach ( var item in encryptedQuery )
            {
                yield return DecryptFiveByFive( item );
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        T DecryptFiveByFive( T item )
        {
            var properties = ToDictionary( new ProjectionProxy( item ) );
            var tokenId = properties["id"];
            var code = properties["code"];
            var hash = properties["hash"];
            var salt = TokenIdentifier.Parse( tokenId.Value ).MintRequestId.ToByteArray();

            using ( var tokenCode = tokenSecurity.DecryptFromBase64( code.Value, hash.Value, salt ) )
            {
                var fiveByFive = SecureStringToGlobalAllocAnsi( tokenCode );

                try
                {
                    code.Value = PtrToStringAnsi( fiveByFive );
                }
                finally
                {
                    if ( fiveByFive != IntPtr.Zero )
                    {
                        ZeroFreeGlobalAllocAnsi( fiveByFive );
                    }
                }
            }

            return item;
        }

        static IReadOnlyDictionary<string, dynamic> ToDictionary( dynamic projection )
        {
            var properties = new Dictionary<string, dynamic>();
            var current = projection.Container;

            do
            {
                properties[current.Name] = current;
            }
            while ( ( current = current.Next ) != null );

            return properties;
        }
    }

    sealed class DecryptedTokenEnumerator : IEnumerable<Token>
    {
        readonly IEnumerable<Token> tokens;
        readonly ITokenSecurity tokenSecurity;

        public DecryptedTokenEnumerator( IEnumerable<Token> tokens, ITokenSecurity tokenSecurity )
        {
            this.tokens = tokens;
            this.tokenSecurity = tokenSecurity;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            foreach ( var token in tokens )
            {
                yield return DecryptFiveByFive( token );
            }
        }

        Token DecryptFiveByFive( Token token )
        {
            var salt = TokenIdentifier.Parse( token.Id ).MintRequestId.ToByteArray();

            using ( var tokenCode = tokenSecurity.DecryptFromBase64( token.Code, token.Hash, salt ) )
            {
                var fiveByFive = SecureStringToGlobalAllocAnsi( tokenCode );

                try
                {
                    token.Code = PtrToStringAnsi( fiveByFive );
                }
                finally
                {
                    if ( fiveByFive != IntPtr.Zero )
                    {
                        ZeroFreeGlobalAllocAnsi( fiveByFive );
                    }
                }
            }

            return token;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}