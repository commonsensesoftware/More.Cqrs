namespace Contoso.Services.Components
{
    using Domain.Accounts;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

    [RuntimeComponent]
    public sealed class AccountFinder : IFindAccount
    {
        public AccountFinder() { }

        public Task<string> FindBusinessAccount( IPrincipal principal, CancellationToken cancellationToken )
        {
            // TODO: extract organization claim (ex: partner id, etc)
            return FromResult( principal.Identity.Name );
        }

        public Task<string> FindConsumerAccount( IPrincipal principal, CancellationToken cancellationToken )
        {
            // TODO: extract user claim (ex: email, PUID, etc)
            return FromResult( principal.Identity.Name );
        }
    }
}