namespace Contoso.Services.Components
{
    using Contoso.Domain.Security;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    [RuntimeComponent]
    public sealed class CertificateLocator : ICertificateLocator
    {
        public X509Certificate2 LocateByAccount( IPrincipal account ) => throw new NotImplementedException();

        public X509Certificate2 LocateByThumbprint( string thumbprint ) => throw new NotImplementedException();
    }
}