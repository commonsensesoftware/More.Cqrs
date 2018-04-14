namespace More.Domain.Example
{
    using More.Domain.Events;
    using More.Domain.Sagas;
    using System;
    using System.Threading.Tasks;
    using static System.DateTimeOffset;
    using static System.Threading.Tasks.Task;

    public class Marriage : Saga<MarriageData>,
        IStartWith<Propose>,
        IReceiveEvent<Engaged>,
        IReceiveEvent<Married>
    {
        readonly IRepository<Guid, Person> people;

        public Marriage( IRepository<Guid, Person> people ) => this.people = people;

        protected override void CorrelateUsing( SagaCorrelator<MarriageData> correlator )
        {
            correlator.Correlate<Propose>( command => command.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<Engaged>( @event => @event.AggregateId ).To( saga => saga.Id );
            correlator.Correlate<Married>( @event => @event.AggregateId ).To( saga => saga.Id );
        }

        public async Task Handle( Propose command, IMessageContext context )
        {
            var person = await people.Single( command.AggregateId, context.CancellationToken );
            var significantOther = await people.Single( command.OtherPersonId, context.CancellationToken );
            var version = significantOther.Version;

            person.ProposeTo( significantOther, UtcNow );

            await people.Save( person, command.ExpectedVersion, context.CancellationToken );
            await people.Save( significantOther, version, context.CancellationToken );

            Data.ProposerId = person.Id;
            Data.SpouseToBeId = significantOther.Id;
        }

        public async Task Receive( Engaged @event, IMessageContext context )
        {
            var person = await people.Single( @event.AggregateId, context.CancellationToken );
            var fiancé = await people.Single( @event.FiancéId, context.CancellationToken );
            var version = fiancé.Version;

            person.Marry( fiancé, UtcNow );

            await people.Save( person, @event.Version, context.CancellationToken );
            await people.Save( fiancé, version, context.CancellationToken );
        }

        public Task Receive( Married @event, IMessageContext context )
        {
            MarkAsComplete();
            return CompletedTask;
        }
    }
}