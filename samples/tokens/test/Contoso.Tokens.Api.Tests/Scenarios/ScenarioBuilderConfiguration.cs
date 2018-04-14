namespace Contoso.Services.Scenarios
{
    using System;

    public class ScenarioBuilderConfiguration
    {
        readonly ScenarioBuilder builder;

        public ScenarioBuilderConfiguration( ScenarioBuilder builder ) => this.builder = builder;

        public ScenarioBuilder Then() => builder;

        public ScenarioBuilderConfiguration PartnerId( string value )
        {
            builder.Data.PartnerId = value;
            return this;
        }

        public ScenarioBuilderConfiguration CatalogId( string value )
        {
            builder.Data.CatalogId = value;
            return this;
        }

        public ScenarioBuilderConfiguration ConsumerId( string value )
        {
            builder.Data.ConsumerId = value;
            return this;
        }
    }
}