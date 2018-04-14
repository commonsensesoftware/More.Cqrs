namespace Contoso.Services.Composition.Conventions
{
    using More.ComponentModel;
    using System;

    sealed class ReadModelRepositoryConvention : IRule<ConventionContext>
    {
        const string PerRequest = nameof( PerRequest );
        readonly ISpecification<Type> Repository;
        readonly ISpecification<Type> Assembly;

        internal ReadModelRepositoryConvention()
        {
            var type = typeof( IReadOnlyRepository<> );
            Repository = new InterfaceSpecification( type );
            Assembly = new PublicKeyTokenSpecification( type );
        }

        public void Evaluate( ConventionContext context ) =>
            context.Conventions
                   .ForTypesMatching( Repository.IsSatisfiedBy )
                   .ExportInterfaces( Assembly.IsSatisfiedBy );
        // TODO: is sharing really needed for read models? sharing can affect affect other uses (ex: TokenVault in MintPressRun saga)
        //.Shared( PerRequest );
    }
}