namespace Contoso.Services.Composition.Conventions
{
    using More.ComponentModel;
    using More.Domain;
    using More.Domain.Events;
    using More.Domain.Persistence;
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;

    sealed class AggregateConvention : IRule<ConventionContext>
    {
        readonly Type IAggregateOfT = typeof( IAggregate<> );
        readonly Type IPersistence = typeof( IPersistence );
        readonly Type SqlPersistence = typeof( SqlPersistence );
        readonly Type IEventStoreOfT = typeof( IEventStore<> );
        readonly Type SqlEventStoreOfT = typeof( SqlEventStore<> );
        readonly Type RepositoryOfT = typeof( Repository<,> );
        readonly Type IRepositoryOfT = typeof( IRepository<,> );
        readonly ISpecification<Type> Aggregate;

        internal AggregateConvention() => Aggregate = new InterfaceSpecification( IAggregateOfT );

        ConstructorSelectionRule EventStoreConstructorSelector { get; } = new ConstructorSelectionRule( typeof( SqlPersistence ) );

        public void Evaluate( ConventionContext context )
        {
            foreach ( var aggregateType in context.Types.ConcreteClasses().Where( Aggregate.IsSatisfiedBy ) )
            {
                var interfaceType = aggregateType.GetInterface( IAggregateOfT.FullName );
                var keyType = interfaceType.GenericTypeArguments[0];
                var contractName = aggregateType.Name;

                ApplyEventStoreConvention( context, aggregateType, keyType, contractName );
            }
        }

        void ApplyEventStoreConvention( ConventionContext context, Type aggregateType, Type keyType, string contractName )
        {
            Contract.Requires( context != null );
            Contract.Requires( aggregateType != null );
            Contract.Requires( keyType != null );
            Contract.Requires( !string.IsNullOrEmpty( contractName ) );

            var type = SqlEventStoreOfT.MakeGenericType( keyType );
            var contractType = IEventStoreOfT.MakeGenericType( keyType );
            var conventions = context.Conventions;

            context.ClosedGenericTypes.Add( type );

            // [Export( nameof( TAggregate ), typeof( IEventStore<TKey> ) )]
            // public class SqlEventStore<TKey>( [Import( nameof( TAggregate ) )] SqlPersistence persistence ) { }
            conventions.ForType( type )
                       .Export( export => export.AsContractType( contractType ).AsContractName( contractName ) )
                       .SelectConstructor( EventStoreConstructorSelector.Evaluate, ( p, import ) => import.AsContractName( contractName ) )
                       .Shared();

            ApplyRepositoryConvention( context, aggregateType, keyType, contractName );
        }

        void ApplyRepositoryConvention( ConventionContext context, Type aggregateType, Type keyType, string contractName )
        {
            Contract.Requires( context != null );
            Contract.Requires( aggregateType != null );
            Contract.Requires( keyType != null );
            Contract.Requires( !string.IsNullOrEmpty( contractName ) );

            var type = RepositoryOfT.MakeGenericType( keyType, aggregateType );
            var contractType = IRepositoryOfT.MakeGenericType( keyType, aggregateType );
            var conventions = context.Conventions;

            context.ClosedGenericTypes.Add( type );

            // [Export( nameof( TAggregate ), typeof( IRepository<TKey, TAggregate> ) )]
            // public class Repository<TKey, TAggregate>( [Import( nameof( TAggregate ) )] IEventStore<TKey> eventStore ) { }
            conventions.ForType( type )
                       .Export( export => export.AsContractType( contractType ) )
                       .SelectConstructor( ctors => ctors.Single(), ( p, import ) => import.AsContractName( contractName ) )
                       .Shared();
        }
    }
}