using System;
using NWheels.Api;
using NWheels.Api.Ddd;

namespace ExpenseTracker.Domain.Entities
{
    [DomainModel.ValueObject]
    public class BudgetEntry
    {
        public BudgetEntry(Category category)
        {
            this.Category = category;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedProperty]
        public virtual Category Category { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedProperty]
        public virtual decimal Budget { get; protected set; }    

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedProperty]
        public virtual decimal Expense { get; protected set; }    
    }
}