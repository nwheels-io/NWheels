using System;
using NWheels.Api;
using NWheels.Api.Ddd;

namespace ExpenseTracker.Domain
{
    [DomainModel.Entity(IsAggregateRoot = true)]
    public class Budget
    {
        private readonly IFramework _framework;
        private readonly ILocalizables _localizables;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Budget(IFramework framework, ILocalizables localizables)
        {
            _framework = framework;
            _localizables = localizables;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.EntityId]
        public virtual YearMonth YearMonth { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.Relation.Composition, DomainModel.Invariant.Required, DomainModel.PersistedValue]
        public virtual Category RootCategory { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.Constructor]
        protected virtual void Constructor()
        {
            RootCategory = _framework.NewDomainObject<Category>(initializer: c => {
                c.Name = _localizables.AllExpenses;
            });
        }
    }
}
