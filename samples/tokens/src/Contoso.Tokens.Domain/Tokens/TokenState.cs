namespace Contoso.Domain.Tokens
{
    using System;

    enum TokenState
    {
        Minted,
        Circulated,
        Reserved,
        Activated,
        Redeemed,
        Voided,
    }
}