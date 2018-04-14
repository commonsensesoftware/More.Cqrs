namespace Contoso.Services.Configuration
{
    using System;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;
    using static System.String;
    using static System.Threading.Tasks.Task;

    internal sealed class SimpleAuthentiationFilter : IAuthenticationFilter
    {
        const string DefaultIdentity = "anonymous@somewhere.com";

        public bool AllowMultiple { get; } = true;

        public Task AuthenticateAsync( HttpAuthenticationContext context, CancellationToken cancellationToken )
        {
            var headers = context.Request.Headers;
            var from = headers.From;

            if ( IsNullOrEmpty( from ) )
            {
                if ( headers.TryGetValues( "from", out var values ) )
                {
                    from = values.FirstOrDefault();

                    if ( IsNullOrEmpty( from ) )
                    {
                        from = DefaultIdentity;
                    }
                }
                else
                {
                    from = DefaultIdentity;
                }
            }

            context.Principal = new GenericPrincipal( new GenericIdentity( from ), Array.Empty<string>() );
            return CompletedTask;
        }

        public Task ChallengeAsync( HttpAuthenticationChallengeContext context, CancellationToken cancellationToken ) => CompletedTask;
    }
}