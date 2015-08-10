using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core;

namespace NWheels.Entities.Factories
{
    public static class RuntimeEntityModelHelpers
    {
        public static 
            ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>
            CreateContainmentCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>(
                ICollection<TContainedContract> innerCollection,
                IDomainObjectFactory domainObjectFactory)
                where TContained : class, IContainedIn<TContainer>
                where TContainer : class, IContain<TContained>
                where TContainedContract : class
                where TContainerContract : class
        {
            return new ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>(
                innerCollection,
                persistable => (TContainer)(object)domainObjectFactory.CreateDomainObjectInstance<TContainerContract>((TContainerContract)(object)persistable));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static 
            ContainedToContainerListAdapter<TContained, TContainer, TContainedContract, TContainerContract> 
            CreateContainmentListAdapter<TContained, TContainer, TContainedContract, TContainerContract>(
                IList<TContainedContract> innerCollection,
                IDomainObjectFactory domainObjectFactory)
                where TContained : class, IContainedIn<TContainer>
                where TContainer : class, IContain<TContained>
                where TContainedContract : class
                where TContainerContract : class
        {
            return new ContainedToContainerListAdapter<TContained, TContainer, TContainedContract, TContainerContract>(
                innerCollection,
                persistable => (TContainer)(object)domainObjectFactory.CreateDomainObjectInstance<TContainerContract>((TContainerContract)(object)persistable));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void InjectContainerDomainObject(IComponentContext components, IDomainObject domainObject, IPersistableObject persistableObject)
        {
            persistableObject.SetContainerObject(domainObject);

            var hasNestedObjects = (persistableObject as IHaveNestedObjects);

            if ( hasNestedObjects != null )
            {
                var domainObjectFactory = components.Resolve<IDomainObjectFactory>();
                var nestedObjects = new HashSet<object>();
                hasNestedObjects.DeepListNestedObjects(nestedObjects);

                foreach ( var nested in nestedObjects.OfType<IPersistableObject>() )
                {
                    if ( nested.GetContainerObject() == null )
                    {
                        domainObjectFactory.CreateDomainObjectInstance(nested);
                    }
                }
            }
        }
    }
}
