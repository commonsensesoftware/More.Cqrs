namespace More.Domain.Example
{
    using More.Domain.Commands;
    using System;

    public class Propose : Command
    {
        public Propose( Guid personId, Guid otherPersonId )
        {
            AggregateId = personId;
            OtherPersonId = otherPersonId;
        }

        public Guid OtherPersonId { get; }
    }
}