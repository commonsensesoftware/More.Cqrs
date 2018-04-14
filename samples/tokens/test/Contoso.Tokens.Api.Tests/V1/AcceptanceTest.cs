namespace Contoso.Services.V1
{
    using Microsoft.Web.Http;
    using System;
    using Xunit;
    using Xunit.Abstractions;

    [Trait( "Version", "1.0" )]
    public abstract class AcceptanceTest : ScenarioTest
    {
        protected AcceptanceTest( ITestOutputHelper output ) : base( output ) { }

        protected override ApiVersion ApiVersion { get; } = new ApiVersion( 1, 0 );
    }
}