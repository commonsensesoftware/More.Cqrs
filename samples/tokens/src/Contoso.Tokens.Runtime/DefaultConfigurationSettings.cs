namespace Contoso.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    sealed class DefaultConfigurationSettings : IEnumerable<KeyValuePair<string, string>>
    {
        internal const string ConnectionStringKey = "contoso:tokens:connectionString";
        internal const string SubscriptionKey = "contoso:tokens:defaultSubscriptionId";

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            yield return new KeyValuePair<string, string>( ConnectionStringKey, @"server=(localdb)\mssqllocaldb;database=ContosoTokens;trusted_connection=true" );
            yield return new KeyValuePair<string, string>( SubscriptionKey, "f4bcb375-20a7-4e4c-81eb-3ba200544548" );
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}