namespace Contoso.Domain.Security
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    public interface ICertificateLocator
    {
        X509Certificate2 LocateByThumbprint( string thumbprint );

        X509Certificate2 LocateByAccount( IPrincipal account );
    }
}