// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using static Newtonsoft.Json.NullValueHandling;
    using static Newtonsoft.Json.TypeNameHandling;

    static class JsonSettings
    {
        internal static JsonSerializerSettings Default { get; } = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy( processDictionaryKeys: true, overrideSpecifiedNames: true ),
            },
            NullValueHandling = Ignore,
            TypeNameHandling = None,
        };
    }
}