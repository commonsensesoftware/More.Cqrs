namespace More.Domain.Example
{
    using System;

    public class Person : Aggregate, IEquatable<Person>
    {
        string firstName;
        string lastName;
        MaritalStatus maritalStatus = MaritalStatus.Single;
        Guid? fiancéId;
        Guid? spouseId;
        DateTimeOffset birthday;
        DateTimeOffset? anniversary;

        public Person() { }

        public Person( Guid id, string firstName, string lastName, DateTimeOffset birthday )
        {
            Record( new Born( id, firstName, lastName, birthday ) );
        }

        public static bool operator ==( Person person, Person other ) =>
            person is null ? other is null : person.Equals( other );

        public static bool operator !=( Person person, Person other ) =>
            person is null ? !( other is null ) : !person.Equals( other );

        public bool Equals( Person other ) =>
            other != null &&
            Id == other.Id &&
            firstName == other.firstName &&
            lastName == other.lastName &&
            birthday == other.birthday &&
            maritalStatus == other.maritalStatus &&
            fiancéId == other.fiancéId &&
            spouseId == other.spouseId &&
            anniversary == other.anniversary;

        public override bool Equals( object obj ) => obj is Person other && Equals( other );

        public override int GetHashCode() => base.GetHashCode();

        public void ProposeTo( Person significantOther, DateTimeOffset date )
        {
            significantOther.AcceptProposal( this, date );

            switch ( maritalStatus )
            {
                case MaritalStatus.Single:
                case MaritalStatus.Divorced:
                case MaritalStatus.Widowed:
                    Record( new Engaged( Id, significantOther.Id, date ) );
                    break;
                case MaritalStatus.Engaged:
                case MaritalStatus.Married:
                    if ( significantOther.Id != fiancéId )
                    {
                        throw new MonogamyException();
                    }
                    break;
            }
        }

        void AcceptProposal( Person significantOther, DateTimeOffset date )
        {
            switch ( maritalStatus )
            {
                case MaritalStatus.Single:
                case MaritalStatus.Divorced:
                case MaritalStatus.Widowed:
                    Record( new Engaged( Id, significantOther.Id, date ) );
                    break;
                case MaritalStatus.Engaged:
                case MaritalStatus.Married:
                    if ( significantOther.Id != fiancéId )
                    {
                        throw new MonogamyException();
                    }
                    break;
            }
        }

        public void Marry( Person spouse, DateTimeOffset date )
        {
            if ( spouseId != spouse.Id )
            {
                Record( new Married( Id, spouse.Id, date ) );
            }

            if ( spouse.spouseId == Id )
            {
                return;
            }

            var me = this;
            spouse.Marry( me, date );
        }

        public void ChangeLastName( string newLastName )
        {
            if ( !StringComparer.OrdinalIgnoreCase.Equals( lastName, newLastName ) )
            {
                Record( new LastNameChanged( Id, newLastName ) );
            }
        }

        void Apply( Born @event )
        {
            Id = @event.AggregateId;
            firstName = @event.FirstName;
            lastName = @event.LastName;
            birthday = @event.Date;
        }

        void Apply( Engaged @event )
        {
            fiancéId = @event.FiancéId;
            maritalStatus = MaritalStatus.Engaged;
        }

        void Apply( Married @event )
        {
            spouseId = @event.SpouseId;
            anniversary = @event.Date;
            maritalStatus = MaritalStatus.Married;
        }

        void Apply( LastNameChanged @event ) => lastName = @event.NewLastName;
    }
}