namespace Contoso.Domain.Accounts
{
    using System;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFindAccount
    {
        Task<string> FindBusinessAccount( IPrincipal principal, CancellationToken cancellationToken );

        Task<string> FindConsumerAccount( IPrincipal principal, CancellationToken cancellationToken );
    }
}