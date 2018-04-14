namespace Contoso.Domain.Tokens.Minting
{
    using System;

    public interface ITokenDie
    {
        void Strike( TokenPlanchet planchet );
    }
}
