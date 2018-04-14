namespace Contoso.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public sealed class TestSequence : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>( IEnumerable<TTestCase> testCases ) where TTestCase : ITestCase
        {
            var orderedTestCases = new SortedDictionary<int, List<TTestCase>>();

            foreach ( var testCase in testCases )
            {
                var attribute = testCase.TestMethod.Method.GetCustomAttributes( typeof( StepAttribute ) ).SingleOrDefault();
                var step = attribute?.GetNamedArgument<int>( nameof( StepAttribute.Number ) ) ?? 0;
                GetOrAdd( orderedTestCases, step ).Add( testCase );
            }

            foreach ( var step in orderedTestCases )
            {
                foreach ( var testCase in step.Value )
                {
                    yield return testCase;
                }
            }
        }

        static TValue GetOrAdd<TKey, TValue>( IDictionary<TKey, TValue> dictionary, TKey key ) where TValue : new()
        {
            if ( dictionary.TryGetValue( key, out var value ) )
            {
                return value;
            }

            dictionary[key] = value = new TValue();
            return value;
        }
    }
}