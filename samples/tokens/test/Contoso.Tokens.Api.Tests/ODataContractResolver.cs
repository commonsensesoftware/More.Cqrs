namespace Contoso.Services
{
    using Newtonsoft.Json.Serialization;
    using System;
    using static System.Text.RegularExpressions.RegexOptions;

    sealed class ODataContractResolver : DefaultContractResolver
    {
        const string CSharpIdentifierForODataAnnotationPattern = "_?odata_";
        const string ODataAnnotation = "@odata.";

        protected override string ResolvePropertyName( string propertyName ) =>
            propertyName.Replace( CSharpIdentifierForODataAnnotationPattern, ODataAnnotation, Singleline );
    }
}