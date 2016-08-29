using System;
using NWheels.Api;
using NWheels.Api.Ddd;

namespace ExpenseTracker.Domain
{
    [DomainModel.Entity]
    public class Payee
    {
        [DomainModel.EntityId]
        public virtual Guid Id { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue, 
            DomainModel.Invariant.Required, 
            DomainModel.Invariant.Unique]
        public virtual string Name { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue, DomainModel.Relation.AggregationParent]
        public virtual CategoryReference DefaultCategory { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class PayeeReference : EntityReference<Payee, Guid> { };
}
