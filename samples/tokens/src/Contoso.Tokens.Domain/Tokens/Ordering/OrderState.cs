namespace Contoso.Domain.Tokens.Ordering
{
    using System;

    public enum OrderState
    {
        Placed,
        Processing,
        Fulfilled,
        Canceled,
    }
}