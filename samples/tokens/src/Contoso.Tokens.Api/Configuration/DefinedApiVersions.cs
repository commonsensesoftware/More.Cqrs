namespace Contoso.Services.Configuration
{
    using Microsoft.Web.Http;
    using System;

    static class DefinedApiVersions
    {
        internal static readonly ApiVersion V1 = new ApiVersion( 1, 0 );
    }
}