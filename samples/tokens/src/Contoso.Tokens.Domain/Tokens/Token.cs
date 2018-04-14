namespace Contoso.Domain.Tokens
{
    using More.Domain;
    using System;
    using static TokenState;

    public class Token : Aggregate<string>
    {
        string code;
        string hash;
        string reservedByBillingAccountId;
        string redeemedByAccountId;
        Guid reservedForOrderId;
        TokenState state;

        public Token( string id, Guid mintRequestId, string code, string hash, string catalogId, long sequenceNumber )
        {
            Record( new TokenMinted( id, mintRequestId, code, hash, catalogId, sequenceNumber ) );
        }

        public void Circulate()
        {
            if ( state == Minted )
            {
                Record( new TokenCirculated( Id ) );
            }
        }

        public void Reserve( Guid orderId, string billingAccountId )
        {
            switch ( state )
            {
                case Minted:
                    throw new TokenNotInCirculationException();
                case Circulated:
                    Record( new TokenReserved( Id, orderId, billingAccountId, code, hash ) );
                    break;
                case Reserved:
                    if ( reservedForOrderId != orderId )
                    {
                        throw new TokenAlreadyReservedException();
                    }
                    break;
                case Activated:
                    throw new TokenAlreadyActivatedException();
                case Redeemed:
                    throw new TokenAlreadyRedeemedException();
                case Voided:
                    throw new TokenVoidedException();
            }
        }

        public void Unreserve( Guid orderId )
        {
            switch ( state )
            {
                case Reserved:
                case Activated:
                    if ( reservedForOrderId == orderId )
                    {
                        Record( new TokenUnreserved( Id, orderId ) );
                    }
                    break;
                case Redeemed:
                    throw new TokenAlreadyRedeemedException();
                case Voided:
                    throw new TokenVoidedException();
            }
        }

        public void Activate( string billingAccountId, Guid? orderId )
        {
            switch ( state )
            {
                case Minted:
                    throw new TokenNotInCirculationException();
                case Circulated:
                    Record( new TokenActivated( Id, billingAccountId, orderId ) );
                    break;
                case Reserved:
                    if ( reservedByBillingAccountId == billingAccountId )
                    {
                        Record( new TokenActivated( Id, billingAccountId, orderId ) );
                    }
                    else
                    {
                        throw new TokenAlreadyReservedException();
                    }
                    break;
                case Activated:
                    if ( reservedByBillingAccountId != billingAccountId )
                    {
                        throw new TokenAlreadyActivatedException();
                    }
                    break;
                case Redeemed:
                    throw new TokenAlreadyRedeemedException();
                case Voided:
                    throw new TokenVoidedException();
            }
        }

        public void Deactivate( Guid? orderId )
        {
            switch ( state )
            {
                case Redeemed:
                    throw new TokenAlreadyRedeemedException();
                case Activated:
                    Record( new TokenDeactivated( Id, orderId ) );
                    break;
            }
        }

        public void Redeem( string accountId )
        {
            switch ( state )
            {
                case Voided:
                    throw new TokenVoidedException();
                case Activated:
                    if ( redeemedByAccountId != accountId && redeemedByAccountId != null )
                    {
                        throw new TokenAlreadyRedeemedException();
                    }
                    Record( new TokenRedeemed( Id, accountId ) );
                    break;
                case Redeemed:
                    if ( redeemedByAccountId != accountId && redeemedByAccountId != null )
                    {
                        throw new TokenAlreadyRedeemedException();
                    }
                    break;
                default:
                    throw new TokenNotActivatedException();
            }
        }

        public void Void()
        {
            if ( state != Voided )
            {
                Record( new TokenVoided( Id ) );
            }
        }

        void Apply( TokenMinted @event )
        {
            Id = @event.AggregateId;
            code = @event.Code;
            hash = @event.Hash;
            state = Minted;
        }

        void Apply( TokenCirculated @event ) => state = Circulated;

        void Apply( TokenReserved @event )
        {
            state = Reserved;
            reservedByBillingAccountId = @event.BillingAccountId;
            reservedForOrderId = @event.OrderId;
        }

        void Apply( TokenUnreserved @event )
        {
            state = Circulated;
            reservedByBillingAccountId = null;
            reservedForOrderId = default( Guid );
        }

        void Apply( TokenActivated @event )
        {
            state = Activated;
            reservedByBillingAccountId = @event.BillingAccountId;
        }

        void Apply( TokenDeactivated @event ) => state = reservedByBillingAccountId == null ? Circulated : Reserved;

        void Apply( TokenRedeemed @event )
        {
            state = Redeemed;
            redeemedByAccountId = @event.AccountId;
        }

        void Apply( TokenVoided @event ) => state = Voided;
    }
}