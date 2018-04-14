namespace Contoso.Domain.Tokens.Minting
{
    using System;

    public enum MintingState
    {
        Started,
        Completed,
        Canceling,
        Canceled,
    }
}