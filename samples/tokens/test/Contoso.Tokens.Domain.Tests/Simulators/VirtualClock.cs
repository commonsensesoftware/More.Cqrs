namespace Contoso.Domain.Simulators
{
    using More.Domain;
    using System;

    public class VirtualClock : IClock
    {
        Func<DateTimeOffset> now = () => DateTimeOffset.Now;

        public DateTimeOffset Now => now();

        public void Reset() => now = () => DateTimeOffset.Now;

        public void AdvanceTo( DateTimeOffset when )
        {
            var then = DateTimeOffset.Now;
            now = () => when + ( DateTimeOffset.Now - then );
        }

        public void AdvanceBy( TimeSpan time )
        {
            var then = now;
            now = () => then() + time;
        }

        public void RewindTo( DateTimeOffset when )
        {
            var then = DateTimeOffset.Now;
            now = () => when + ( DateTimeOffset.Now - then );
        }

        public void RewindBy( TimeSpan time )
        {
            var then = now;
            now = () => then() - time;
        }
    }
}