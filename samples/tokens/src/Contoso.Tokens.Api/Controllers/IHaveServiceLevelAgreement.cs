namespace Contoso.Services.Controllers
{
    using System;

    public interface IHaveServiceLevelAgreement
    {
        TimeSpan Timeout { get; }
    }
}