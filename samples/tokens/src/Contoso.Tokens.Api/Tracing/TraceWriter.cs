namespace Contoso.Services.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Tracing;
    using static System.Diagnostics.Debug;
    using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

    sealed class TraceWriter : ITraceWriter
    {
        public void Trace( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction )
        {
            if ( traceAction == null )
            {
                return;
            }

            AlwaysTraceDuringDebugging( request, category, level, traceAction );

            // TODO: implement tracing

            var traceRecord = new TraceRecord( request, category, level );
        }

        [Conditional( "DEBUG" )]
        static void AlwaysTraceDuringDebugging( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction )
        {
            var traceRecord = new TraceRecord( request, category, level );
            WriteLine( $"{level} : {category} : {traceRecord.Message}" );
        }
    }
}