namespace Contoso.Services.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Composition.Hosting.Core;
    using System.Linq;
    using System.Web.Http.Dependencies;
    using static System.Composition.Hosting.Core.ExportDescriptor;

    sealed class SelfExportDescriptorProvider : ExportDescriptorProvider
    {
        readonly Type[] supportedContracts = new[] { typeof( IDependencyResolver ), typeof( IServiceProvider ) };
        readonly CompositionDependencyResolver self;

        internal SelfExportDescriptorProvider( CompositionDependencyResolver self ) => this.self = self;

        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            if ( contract.ContractName != null )
            {
                return NoExportDescriptors;
            }

            if ( !supportedContracts.Any( supportedContract => supportedContract.Equals( contract.ContractType ) ) )
            {
                return NoExportDescriptors;
            }

            if ( contract.MetadataConstraints == null || !contract.MetadataConstraints.Any() )
            {
                return ExportSelfAsPart( contract, descriptorAccessor );
            }

            return NoExportDescriptors;
        }

        IEnumerable<ExportDescriptorPromise> ExportSelfAsPart( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            yield return new ExportDescriptorPromise(
                                contract,
                                nameof( CompositionDependencyResolver ),
                                true,
                                NoDependencies,
                                _ => Create( ( c, o ) => self, NoMetadata ) );
        }
    }
}