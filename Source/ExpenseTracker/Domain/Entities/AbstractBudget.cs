using System;
using System.Collections.Generic;
using NWheels.Api;
using NWheels.Api.Ddd;

namespace ExpenseTracker.Domain.Entities
{
    [DomainModel.Entity]
    public abstract class AbstractBudget
    {
        [DomainModel.PersistedProperty]
        public virtual ICollection<BudgetEntry> Entries { get; }    
    }
}