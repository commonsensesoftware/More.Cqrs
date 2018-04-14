namespace Contoso.Tokens
{
    using System;

    public enum TokenState
    {
        Minted,
        Circulated,
        Reserved,
        Activated,
        Redeemed,
        Voided,
    }
}