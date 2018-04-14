namespace Contoso.Services
{
    using More.ComponentModel;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.OData.Query;
    using static System.Linq.Expressions.Expression;

    static class IQueryableExtensions
    {
        readonly static Lazy<MethodInfo> EnumerateAsyncOfT = new Lazy<MethodInfo>( () => typeof( IQueryableExtensions ).GetRuntimeMethods().Single( m => m.Name == nameof( EnumerateAsync ) ) );
        readonly static ConcurrentDictionary<Type, Func<IDbAsyncEnumerable, CancellationToken, Task<IEnumerable>>> asyncEnumerables = new ConcurrentDictionary<Type, Func<IDbAsyncEnumerable, CancellationToken, Task<IEnumerable>>>();

        internal static async Task<IEnumerable<TChild>> SubqueryAsync<TEntity, TChild>(
            this IReadOnlyRepository<TEntity> repository,
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, ICollection<TChild>>> property,
            CancellationToken cancellationToken )
            where TEntity : class
            where TChild : class
        {
            if ( !( repository is DbContext context ) )
            {
                return await repository.GetAsync( q => q.Where( predicate ).Select( property ).SelectMany( i => i ), cancellationToken ).ConfigureAwait( false );
            }

            var entity = await context.Set<TEntity>().SingleOrDefaultAsync( predicate, cancellationToken ).ConfigureAwait( false );

            if ( entity == null )
            {
                return null;
            }

            var query = context.Entry( entity ).Collection( property ).Query();

            if ( query is IDbAsyncEnumerable<TChild> )
            {
                return await query.ToArrayAsync( cancellationToken ).ConfigureAwait( false );
            }

            return query;
        }

        internal static async Task<IEnumerable> SubqueryAsync<TEntity, TChild>(
            this IReadOnlyRepository<TEntity> repository,
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, ICollection<TChild>>> property,
            ODataQueryOptions<TChild> options,
            CancellationToken cancellationToken )
            where TEntity : class
            where TChild : class
        {
            if ( !( repository is DbContext context ) )
            {
                var results = await repository.GetAsync( q => options.ApplyTo( q.Where( predicate ).Select( property ) ), cancellationToken ).ConfigureAwait( false );
                return results.ToArray();
            }

            var entity = await context.Set<TEntity>().SingleOrDefaultAsync( predicate, cancellationToken ).ConfigureAwait( false );

            if ( entity == null )
            {
                return null;
            }

            var query = context.Entry( entity ).Collection( property ).Query();
            var subquery = options.ApplyTo( query );

            if ( query is IDbAsyncEnumerable<TChild> typedEnumerable )
            {
                return await query.ToArrayAsync( cancellationToken ).ConfigureAwait( false );
            }
            else if ( query is IDbAsyncEnumerable untypedEnumerable )
            {
                var enumerate = asyncEnumerables.GetOrAdd( query.ElementType, NewEnumerator );

                if ( asyncEnumerables.Count > 100 )
                {
                    asyncEnumerables.Clear();
                }

                return await enumerate( untypedEnumerable, cancellationToken ).ConfigureAwait( false );
            }

            return subquery.ToArray();
        }

        static Func<IDbAsyncEnumerable, CancellationToken, Task<IEnumerable>> NewEnumerator( Type elementType )
        {
            var enumerable = Parameter( typeof( IDbAsyncEnumerable ), "enumerable" );
            var cancellationToken = Parameter( typeof( CancellationToken ), "cancellationToken" );
            var body = Call( EnumerateAsyncOfT.Value.MakeGenericMethod( elementType ), enumerable, cancellationToken );
            var lambda = Lambda<Func<IDbAsyncEnumerable, CancellationToken, Task<IEnumerable>>>( body, enumerable, cancellationToken );

            return lambda.Compile();
        }

        static async Task<IEnumerable> EnumerateAsync<TEntity>( IDbAsyncEnumerable enumerable, CancellationToken cancellationToken )
        {
            using ( var iterator = enumerable.GetAsyncEnumerator() )
            {
                if ( !await iterator.MoveNextAsync( cancellationToken ).ConfigureAwait( false ) )
                {
                    return Array.Empty<TEntity>();
                }

                var results = new List<TEntity>() { (TEntity) iterator.Current };

                while ( await iterator.MoveNextAsync( cancellationToken ).ConfigureAwait( false ) )
                {
                    results.Add( (TEntity) iterator.Current );
                }

                return results;
            }
        }
    }
}