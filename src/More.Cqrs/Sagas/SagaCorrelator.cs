// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Reflection;

    sealed class SagaCorrelator : ICorrelateSagaToMessage
    {
        internal IList<SagaToMessageMap> Mappings { get; } = new List<SagaToMessageMap>();

        public void Configure<TData, TMessage>( Expression<Func<TData, object>> sagaDataProperty, Expression<Func<TMessage, object>> messageProperty )
        {
            Arg.NotNull( sagaDataProperty, nameof( sagaDataProperty ) );
            Arg.NotNull( messageProperty, nameof( messageProperty ) );

            var sagaDataPropertyInfo = GetProperty( sagaDataProperty );
            var messagePropertyInfo = GetProperty( messageProperty );

            EnsurePropertiesAreCompatible( messagePropertyInfo, sagaDataPropertyInfo );

            var getter = messageProperty.Compile();
            var readMessageProperty = new Func<object, object>( message => getter( (TMessage) message ) );

            Mappings.Add( new SagaToMessageMap( typeof( TMessage ), messagePropertyInfo, sagaDataPropertyInfo, readMessageProperty ) );
        }

        static PropertyInfo GetProperty<TObject>( Expression<Func<TObject, object>> expression )
        {
            Contract.Requires( expression != null );
            Contract.Ensures( Contract.Result<PropertyInfo>() != null );

            var memberExpression = default( MemberExpression );

            if ( expression.Body.NodeType == ExpressionType.Convert )
            {
                memberExpression = ( (UnaryExpression) expression.Body ).Operand as MemberExpression;
            }
            else
            {
                memberExpression = expression.Body as MemberExpression;
            }

            var member = memberExpression?.Member;

            if ( member == null )
            {
                throw new ArgumentException( SR.InvalidPropertyExpression.FormatDefault( expression ) );
            }

            var property = member as PropertyInfo;

            if ( property == null )
            {
                throw new ArgumentException( SR.InvalidSagaPropertyExpression.FormatDefault( member.Name, member.DeclaringType.Name ) );
            }

            return property;
        }

        static void EnsurePropertiesAreCompatible( PropertyInfo messageProperty, PropertyInfo sagaDataProperty )
        {
            Contract.Requires( messageProperty != null );
            Contract.Requires( sagaDataProperty != null );

            var leftProperty = messageProperty.PropertyType.GetTypeInfo();
            var rightProperty = sagaDataProperty.PropertyType.GetTypeInfo();

            if ( !rightProperty.IsAssignableFrom( leftProperty ) && !leftProperty.IsAssignableFrom( rightProperty ) )
            {
                throw new InvalidOperationException(
                    SR.SagaPropertyExpressionIncompatible.FormatDefault(
                        sagaDataProperty.Name,
                        sagaDataProperty.PropertyType.Name,
                        messageProperty.Name,
                        messageProperty.PropertyType.Name ) );
            }
        }
    }
}