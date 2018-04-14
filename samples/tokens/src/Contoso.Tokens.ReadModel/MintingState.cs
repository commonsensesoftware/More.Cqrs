namespace Contoso.Tokens
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