namespace Contoso.Domain.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ITokenVault
    {
        Task Deposit( IEnumerable<Token> tokens, CancellationToken cancellationToken );

        Task<bool> Transfer( TransferRequest request, CancellationToken cancellationToken );

        Task ReverseTransfer( Guid orderId, string correlationId, CancellationToken cancellationToken );

        Task ReleaseToCirculation( Guid mintRequestId, string correlationId, CancellationToken cancellationToken );

        Task RemoveFromCirculation( Guid mintRequestId, string correlationId, CancellationToken cancellationToken );
    }
}