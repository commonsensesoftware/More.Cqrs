namespace Contoso.Domain.Tokens.Printing
{
    using System;

    public enum PrintingState
    {
        Queued,
        ReadyForDownload,
        Printing,
        Printed,
        Canceled,
    }
}