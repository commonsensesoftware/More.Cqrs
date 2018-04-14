namespace Contoso.Services.Composition
{
    using More.Domain.Sagas;
    using System;
    using System.Collections.Generic;
    using System.Composition.Hosting.Core;
    using System.Linq;
    using static System.Composition.Hosting.Core.ExportDescriptor;

    sealed class SagaMetadataExportDescriptorProvider : ExportDescriptorProvider
    {
        readonly Type contractType = typeof( SagaMetadataCollection );
        readonly Lazy<SagaMetadataCollection> import;

        internal SagaMetadataExportDescriptorProvider( ConventionContext context ) =>
            import = new Lazy<SagaMetadataCollection>( () => new SagaMetadataCollection( context.Types.ConcreteClasses() ) );

        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            if ( contract.ContractName != null || !contractType.Equals( contract.ContractType ) )
            {
                return NoExportDescriptors;
            }

            if ( contract.MetadataConstraints == null || !contract.MetadataConstraints.Any() )
            {
                return ExportSagaMetadataPart( contract, descriptorAccessor );
            }

            return NoExportDescriptors;
        }

        IEnumerable<ExportDescriptorPromise> ExportSagaMetadataPart( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            const bool shared = true;
            yield return new ExportDescriptorPromise(
                contract,
                nameof( SagaMetadataExportDescriptorProvider ),
                shared,
                NoDependencies,
                _ => Create( ( c, o ) => import.Value, NoMetadata ) );
        }
    }
}