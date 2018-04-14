namespace Contoso.Services.Components
{
    using More.Domain.Persistence;
    using System.Composition;

    public sealed class RuntimePersistenceMapper : PersistenceMapper
    {
        public RuntimePersistenceMapper( CompositionContext context ) : base( context.GetExport<IPersistence> ) { }
    }
}