namespace More.Domain.Messaging
{
    using Events;
    using System;

    public class SeatReserved : Event
    {
        public SeatReserved( Guid aggregateId, DateTimeOffset reservationTime, string attendee )
        {
            AggregateId = aggregateId;
            ReservationTime = reservationTime;
            Attendee = attendee;
        }

        public DateTimeOffset ReservationTime { get; }

        public string Attendee { get; }
    }
}