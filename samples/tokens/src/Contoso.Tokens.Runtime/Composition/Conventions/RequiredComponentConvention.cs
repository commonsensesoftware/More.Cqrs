namespace Contoso.Services.Composition.Conventions
{
    using Contoso.Domain.Tokens;
    using Contoso.Services.Components;
    using More.ComponentModel;
    using System.Linq;

    sealed class RequiredComponentConvention : IRule<ConventionContext>
    {
        public void Evaluate( ConventionContext context )
        {
            var conventions = context.Conventions;

            conventions.ForType<TokenSecurity>().Export<ITokenSecurity>().Shared();

            conventions.ForType<TokenVault>()
                               .Export<ITokenVault>()
                               .SelectConstructor( ctors => ctors.Single(), ( p, import ) => import.AsContractName( "Token" ) )
                               .Shared();
        }
    }
}