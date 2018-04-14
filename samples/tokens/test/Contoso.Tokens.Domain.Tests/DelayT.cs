namespace Contoso.Domain
{
    using More.Domain.Messaging;
    using System;
    using System.Collections.Generic;

    public sealed class Delay<T> where T : IMessage
    {
        readonly MessageFilter messageFilter;
        readonly IDictionary<Type, TimeSpan> messageDelays;

        public Delay( MessageFilter messageFilter, IDictionary<Type, TimeSpan> messageDelays )
        {
            this.messageFilter = messageFilter;
            this.messageDelays = messageDelays;
        }

        public MessageFilter Until( DateTimeOffset when ) => By( when - DateTime.UtcNow );

        public MessageFilter By( TimeSpan time )
        {
            messageDelays[typeof( T )] = time;
            return messageFilter;
        }
    }
}