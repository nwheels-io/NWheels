using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.Extensions;
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

        public static InnerToOuterCollectionAdapter<TEntityContract> CreatePresentationCollectionAdapter<TEntityContract>(
            ICollection<TEntityContract> domainObjectCollection,
            IPresentationObjectFactory presentationFactory)
            where TEntityContract : class
        {
            return new InnerToOuterCollectionAdapter<TEntityContract>(
                domainObjectCollection,
                innerToOuter: domainObject => {
                    return presentationFactory.CreatePresentationObjectInstance<TEntityContract>(domainObject);
                },
                outerToInner: presentationObject => {
                    return (TEntityContract)((IPresentationObject)presentationObject).GetDomainObject();
                });
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IDomainObject EnsureContainerDomainObject<TEntityContract>(IPersistableObject persistableObject, IComponentContext components)
        {
            var existingDomainObject = persistableObject.GetContainerObject();

            if ( existingDomainObject != null )
            {
                return existingDomainObject;
            }

            var factory = components.Resolve<IDomainObjectFactory>();
            var newDomainObject = factory.CreateDomainObjectInstance<TEntityContract>((TEntityContract)persistableObject);

            return (IDomainObject)newDomainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void SaveActiveRecordObject(IDomainObject domainObject, IFramework framework)
        {
            var coreFramework = (ICoreFramework)framework;

            using ( var dataRepository = coreFramework.NewUnitOfWorkForEntity(domainObject.ContractType) )
            {
                var entityRepository = dataRepository.GetEntityRepository(domainObject.ContractType);
                entityRepository.Save(domainObject);
                dataRepository.CommitChanges();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void DeleteActiveRecordObject(IDomainObject domainObject, IFramework framework)
        {
            var coreFramework = (ICoreFramework)framework;

            using ( var dataRepository = coreFramework.NewUnitOfWorkForEntity(domainObject.ContractType) )
            {
                var entityRepository = dataRepository.GetEntityRepository(domainObject.ContractType);
                entityRepository.Delete(domainObject);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsDomainObjectModified(object obj)
        {
            var domainObject = obj.AsOrNull<IDomainObject>();
            return (domainObject == null || domainObject.IsModified);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TContract GetNestedDomainObject<TContract>(object nestedPersistableObject, IDomainObjectFactory domainObjectFactory)
            where TContract : class
        {
            if ( nestedPersistableObject == null )
            {
                return null;
            }

            var containedObject = (TContract)nestedPersistableObject;
            var containerObject = (TContract)((IPersistableObject)nestedPersistableObject).GetContainerObject();
            
            if ( containerObject != null )
            {
                return containerObject;
            }

            return domainObjectFactory.CreateDomainObjectInstance<TContract>(containedObject);
        }
    }
}
