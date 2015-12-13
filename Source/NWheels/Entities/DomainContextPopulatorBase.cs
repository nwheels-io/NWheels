using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public abstract class DomainContextPopulatorBase<TContext> : IDomainContextPopulator
        where TContext : IApplicationDataRepository
    {
        void IDomainContextPopulator.Populate(IApplicationDataRepository context)
        {
            var typedCondext = (TContext)context;
            
            OnPopulateContext(typedCondext);

            if ( context.UnitOfWorkState != UnitOfWorkState.Committed )
            {
                typedCondext.CommitChanges();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IDomainContextPopulator.ContextType
        {
            get
            {
                return typeof(TContext);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnPopulateContext(TContext context);
    }
}
