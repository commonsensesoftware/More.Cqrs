namespace Contoso.Tokens
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