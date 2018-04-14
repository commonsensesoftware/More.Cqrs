namespace Contoso.Services.Scenarios
{
    using System;
    using System.Collections.Generic;

    public class ScenarioData
    {
        public Guid ClientRequestId { get; set; }

        public string PartnerId { get; set; }

        public string ConsumerId { get; set; }

        public string CatalogId { get; set; }

        public Guid MintRequestId { get; set; }

        public Guid OrderId { get; set; }

        public IList<RetrievedToken> Tokens { get; } = new List<RetrievedToken>();
    }
}