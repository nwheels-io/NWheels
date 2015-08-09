using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;
using NWheels.TypeModel.Core;

namespace NWheels.Entities.Factories
{
    public static class RuntimeEntityModelHelpers
    {
        public static 
            ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>
            CreateContainmentCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>(
                ICollection<TContainedContract> innerCollection)
                where TContained : class, IContainedIn<TContainer>
                where TContainer : class, IContain<TContained>
                where TContainedContract : class
                where TContainerContract : class
        {
            return new ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>(innerCollection);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static 
            ContainedToContainerListAdapter<TContained, TContainer, TContainedContract, TContainerContract> 
            CreateContainmentListAdapter<TContained, TContainer, TContainedContract, TContainerContract>(
                IList<TContainedContract> innerCollection)
                where TContained : class, IContainedIn<TContainer>
                where TContainer : class, IContain<TContained>
                where TContainedContract : class
                where TContainerContract : class
        {
            return new ContainedToContainerListAdapter<TContained, TContainer, TContainedContract, TContainerContract>(innerCollection);
        }
    }
}
