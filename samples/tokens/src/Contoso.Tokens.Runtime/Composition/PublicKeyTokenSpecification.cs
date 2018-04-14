namespace Contoso.Services.Composition
{
    using More.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using static System.Byte;
    using static System.Globalization.CultureInfo;
    using static System.Globalization.NumberStyles;

    sealed class PublicKeyTokenSpecification : SpecificationBase<AssemblyName>, ISpecification<Type>
    {
        readonly Lazy<byte[]> publicKeyToken;

        internal PublicKeyTokenSpecification() { }

        internal PublicKeyTokenSpecification( string publicKeyToken )
        {
            if ( !string.IsNullOrEmpty( publicKeyToken ) )
            {
                this.publicKeyToken = new Lazy<byte[]>( () => FromHex( publicKeyToken ) );
            }
        }

        internal PublicKeyTokenSpecification( byte[] publicKeyToken )
        {
            this.publicKeyToken = new Lazy<byte[]>( () => publicKeyToken );
        }

        internal PublicKeyTokenSpecification( Assembly assembly )
        {
            publicKeyToken = new Lazy<byte[]>( () => assembly.GetName().GetPublicKeyToken() );
        }

        internal PublicKeyTokenSpecification( Type type )
        {
            publicKeyToken = new Lazy<byte[]>( () => type.GetTypeInfo().Assembly.GetName().GetPublicKeyToken() );
        }

        static bool IsNull( byte[] array ) => array?.Length == 0;

        static byte[] FromHex( string token )
        {
            Contract.Requires( !string.IsNullOrEmpty( token ) );
            Contract.Requires( token.Length % 2 == 0 );
            Contract.Ensures( Contract.Result<byte[]>() != null );

            var bytes = new List<byte>();
            var culture = CurrentCulture;

            for ( var i = 0; i < token.Length; i += 2 )
            {
                bytes.Add( Parse( token.Substring( i, 2 ), HexNumber, culture ) );
            }

            return bytes.ToArray();
        }

        public override bool IsSatisfiedBy( AssemblyName item )
        {
            var otherPublicKeyToken = item.GetPublicKeyToken();

            if ( IsNull( publicKeyToken.Value ) )
            {
                return IsNull( otherPublicKeyToken );
            }

            if ( IsNull( otherPublicKeyToken ) )
            {
                return false;
            }

            return publicKeyToken.Value.SequenceEqual( otherPublicKeyToken );
        }

        public bool IsSatisfiedBy( Type item ) => IsSatisfiedBy( item.GetTypeInfo().Assembly.GetName() );

        public ISpecification<Type> And( ISpecification<Type> other ) => new LogicalAndSpecification<Type>( this, other );

        public ISpecification<Type> Or( ISpecification<Type> other ) => new LogicalOrSpecification<Type>( this, other );

        ISpecification<Type> ISpecification<Type>.Not() => new LogicalNotSpecification<Type>( this );

        bool IRule<Type, bool>.Evaluate( Type item ) => IsSatisfiedBy( item );
    }
}