namespace More.Domain.Example
{
    using More.Domain.Sagas;
    using System;

    public class MarriageData : SagaData
    {
        public Guid ProposerId { get; set; }

        public Guid SpouseToBeId { get; set; }
    }
}