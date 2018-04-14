namespace Contoso.Services.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Composition.Hosting.Core;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;

    sealed class HttpRequestExportDescriptorProvider : ExportDescriptorProvider
    {
        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            if ( contract.ContractName != null )
            {
                return NoExportDescriptors;
            }

            if ( !typeof( HttpRequestMessage ).Equals( contract.ContractType ) )
            {
                return NoExportDescriptors;
            }

            if ( contract.MetadataConstraints == null || !contract.MetadataConstraints.Any() )
            {
                return new[] { ExportHttpRequestMessagePart( contract, descriptorAccessor ) };
            }

            return NoExportDescriptors;
        }

        private ExportDescriptorPromise ExportHttpRequestMessagePart( CompositionContract contract, DependencyAccessor descriptorAccessor )
        {
            Contract.Requires( contract != null );
            Contract.Requires( descriptorAccessor != null );
            Contract.Ensures( Contract.Result<ExportDescriptorPromise>() != null );

            const bool shared = true;

            Func<IEnumerable<CompositionDependency>, ExportDescriptor> factory = dependencies => ExportDescriptor.Create( ( context, operation ) => LogicalHttpRequestMessage.Current, NoMetadata );
            var promise = new ExportDescriptorPromise( contract, "WebApi", shared, NoDependencies, factory );

            return promise;
        }
    }
}