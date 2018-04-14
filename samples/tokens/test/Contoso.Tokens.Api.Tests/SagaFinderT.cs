namespace Contoso.Services
{
    using More.Domain.Sagas;
    using System;
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;

    public class SagaFinder<TData> where TData : class, ISagaData
    {
        const string LocalDB = @"server=(localdb)\mssqllocaldb;database=ContosoTokens;trusted_connection=true;application name=Api Tests";

        public SagaPredicate<TData> Where( Expression<Func<TData, bool>> predicate )
        {
            var equals = predicate.Body as BinaryExpression;

            if ( equals == null || equals.NodeType != ExpressionType.Equal )
            {
                throw new ArgumentException( $"Only equality is supported in the body of the expression {predicate}." );
            }

            var propertyName = GetPropertyName( equals.Left );
            var propertyValue = Evaluate( equals.Right );
            var configuration = new TestSqlSagaStorageConfigurationBuilder().HasConnectionString( LocalDB ).CreateConfiguration();

            return new SagaPredicate<TData>( configuration, propertyName, propertyValue );
        }

        static string GetPropertyName( Expression expression )
        {
            var left = expression as MemberExpression ?? throw new ArgumentException( $"The left-hand side of the expression must be a property accessor." );
            return left.Member.Name;
        }

        static object Evaluate( Expression expression )
        {
            if ( expression is ConstantExpression constant )
            {
                return constant.Value;
            }

            var lambda = Lambda( expression );
            var @delegate = lambda.Compile();

            return Constant( @delegate.DynamicInvoke(), expression.Type ).Value;
        }
    }
}