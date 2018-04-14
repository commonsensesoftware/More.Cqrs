namespace Contoso.Text
{
    using System;

    sealed class BitcoinAlphabet : Base58Alphabet
    {
        internal BitcoinAlphabet() : base( "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz" ) { }
    }
}