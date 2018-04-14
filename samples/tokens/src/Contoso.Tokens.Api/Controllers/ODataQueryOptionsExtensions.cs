namespace Contoso.Services.Controllers
{
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.ModelBinding;
    using System.Web.OData;
    using System.Web.OData.Extensions;
    using System.Web.OData.Formatter;
    using System.Web.OData.Query;
    using static Microsoft.OData.Edm.EdmConcurrencyMode;

    static class ODataQueryOptionsExtensions
    {
        const HttpStatusCode Http428 = (HttpStatusCode) 428;

        static Exception BadRequest( this ODataQueryOptions options, string errorCode, string message ) =>
            options.ClientError( HttpStatusCode.BadRequest, errorCode, message );

        static Exception PreconditionRequired( this ODataQueryOptions options, string errorCode, string message ) =>
            options.ClientError( Http428, errorCode, message );

        static Exception PreconditionFailed( this ODataQueryOptions options, string errorCode, string message ) =>
            options.ClientError( HttpStatusCode.PreconditionFailed, errorCode, message );

        static Exception ClientError( this ODataQueryOptions options, HttpStatusCode statusCode, string errorCode, string message )
        {
            Contract.Requires( options != null );
            Contract.Ensures( Contract.Result<Exception>() != null );

            var request = options.Request;
            var error = new ODataError() { ErrorCode = errorCode, Message = message };
            var response = request.CreateResponse( statusCode, error );

            return new HttpResponseException( response );
        }

        static IEdmProperty MapClrPropertyToEdmProperty<TEntity, TValue>( this Expression<Func<TEntity, TValue>> propertyExpression, ODataQueryContext context )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<IEdmProperty>() != null );

            var model = context.Model;
            var entity = (IEdmStructuredType) context.ElementType;
            var propertyInfo = ( (MemberExpression) propertyExpression.Body ).Member;
            var properties = from property in entity.Properties()
                             let annotation = model.GetAnnotationValue<ClrPropertyInfoAnnotation>( property )
                             where annotation.ClrPropertyInfo == propertyInfo
                             select property;

            return properties.Single();
        }

        internal static IEdmProperty MapClrPropertyToEdmProperty<TEntity, TValue>( this ODataQueryOptions<TEntity> options, Expression<Func<TEntity, TValue>> propertyExpression ) =>
            propertyExpression.MapClrPropertyToEdmProperty( options.Context );

        static ETag<TEntity> CreateETag<TEntity>( this ODataQueryOptions<TEntity> options )
        {
            Contract.Requires( options != null );

            // HACK: this method differs in the out-of-the-box OData handling of If-Match in
            // that it used the Context.ElementType instead of Context.Path. The path will
            // not yield the correct entity type when it refers to an entity action.

            var request = options.Request;
            var ifMatch = request.Headers.IfMatch.SingleOrDefault();

            if ( ifMatch == null )
            {
                return null;
            }

            var handler = request.GetConfiguration().GetETagHandler();
            var properties = handler.ParseETag( ifMatch );
            var values = properties.OrderBy( p => p.Key ).Select( p => p.Value ).ToArray();
            var type = (IEdmEntityType) options.Context.ElementType;
            var names = type.StructuralProperties()
                            .Where( p => p.ConcurrencyMode == Fixed && p.Type.IsPrimitive() )
                            .OrderBy( p => p.Name )
                            .Select( p => p.Name )
                            .ToArray();
            var etag = new ETag<TEntity>() { IsWellFormed = names.Length == values.Length };

            foreach ( var pair in names.Zip( values, ( name, value ) => Tuple.Create( name, value ) ) )
            {
                etag[pair.Item1] = pair.Item2;
            }

            return etag;
        }

        internal static TValue ETagValue<TEntity, TValue>( this ODataQueryOptions<TEntity> options, Expression<Func<TEntity, TValue>> propertyExpression )
        {
            Contract.Requires( options != null );
            Contract.Requires( propertyExpression != null );

            var ifMatch = options.IfMatch ?? options.CreateETag();

            if ( ifMatch == null )
            {
                throw options.PreconditionRequired( "MissingEntityTag", "An entity tag must be specified in the If-Match header." );
            }

            if ( !ifMatch.IsAny && ifMatch.IsWellFormed )
            {
                var property = propertyExpression.MapClrPropertyToEdmProperty( options.Context );

                try
                {
                    return ifMatch.GetValue<TValue>( property.Name );
                }
                catch
                {
                }
            }

            throw options.PreconditionFailed( "InvalidEntityTag", "The entity tag specified in the If-Match header is invalid." );
        }

        internal static bool IsValidField<TEntity, TValue>(
            this ODataQueryOptions<TEntity> options,
            Expression<Func<TEntity, TValue>> propertyExpression,
            ModelStateDictionary modelState )
        {
            Contract.Requires( options != null );
            Contract.Requires( propertyExpression != null );

            var key = "newRequest." + ( (MemberExpression) propertyExpression.Body ).Member.Name;
            return modelState.IsValidField( key );
        }
    }
}