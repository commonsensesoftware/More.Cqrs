namespace Contoso.Domain
{
    using More.Domain.Messaging;
    using System;

    interface IScenario
    {
        IMessage Create();
    }
}