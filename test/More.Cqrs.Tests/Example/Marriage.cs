namespace More.Domain.Example
{
    using More.Domain.Events;
    using More.Domain.Sagas;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.DateTimeOffset;

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

        public async ValueTask Handle( Propose command, IMessageContext context, CancellationToken cancellationToken )
        {
            var person = await people.Single( command.AggregateId, cancellationToken );
            var significantOther = await people.Single( command.OtherPersonId, cancellationToken );
            var version = significantOther.Version;

            person.ProposeTo( significantOther, UtcNow );

            await people.Save( person, command.ExpectedVersion, cancellationToken );
            await people.Save( significantOther, version, cancellationToken );

            Data.ProposerId = person.Id;
            Data.SpouseToBeId = significantOther.Id;
        }

        public async ValueTask Receive( Engaged @event, IMessageContext context, CancellationToken cancellationToken )
        {
            var person = await people.Single( @event.AggregateId, cancellationToken );
            var fiancé = await people.Single( @event.FiancéId, cancellationToken );
            var version = fiancé.Version;

            person.Marry( fiancé, UtcNow );

            await people.Save( person, @event.Version, cancellationToken );
            await people.Save( fiancé, version, cancellationToken );
        }

        public ValueTask Receive( Married @event, IMessageContext context, CancellationToken cancellationToken )
        {
            MarkAsComplete();
            return default;
        }
    }
}