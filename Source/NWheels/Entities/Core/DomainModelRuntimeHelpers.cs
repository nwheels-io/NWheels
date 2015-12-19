using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;

namespace NWheels.Entities.Core
{
    public class DomainModelRuntimeHelpers
    {
        public static TContract ImportEmbeddedDomainObject<TContract, TImpl>(IEntityRepository entityRepo, object importValue, Func<TImpl> implFactory)
            where TImpl : TContract
        {
            var impl = implFactory();
            var persisted = ((IPersistableObject)importValue);
            if ( persisted != null )
            {
                ((IDomainObject)impl).ImportValues(entityRepo, persisted.ExportValues(entityRepo));
            }
            else
            {
                ((IDomainObject)impl).InitializeValues(idManuallyAssigned: false);
            }
            return impl;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static IList<TContract> ImportEmbeddedDomainCollection<TContract, TImpl>(IEntityRepository entityRepo, object importValue, Func<TImpl> itemImplFactory)
            where TImpl : TContract
        {
            var persistedCollection = (IEnumerable<IPersistableObject>)importValue;
            var concrete = new List<TImpl>();

            if ( persistedCollection != null )
            {
                concrete.AddRange(persistedCollection.Select(persistedItem =>
                {
                    var impl = itemImplFactory();
                    if ( persistedItem != null )
                    {
                        ((IDomainObject)impl).ImportValues(entityRepo, persistedItem.ExportValues(entityRepo));
                    }
                    else
                    {
                        ((IDomainObject)impl).InitializeValues(idManuallyAssigned: false);
                    }
                    return impl;
                }));
            }
            return new ConcreteToAbstractListAdapter<TImpl, TContract>(concrete);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static TContract ImportDomainLazyLoadObject<TContract, TImpl>(IEntityRepository entityRepo, object importValue, Func<TImpl> implFactory)
            where TContract : class
            where TImpl : TContract
        {
            var persisted = (importValue as IPersistableObject);
            var lazyLoader = (importValue as IPersistableObjectLazyLoader);

            if ( persisted != null )
            {
                var impl = implFactory();
                ((IDomainObject)impl).ImportValues(entityRepo, persisted.ExportValues(entityRepo));
                return impl;
            }
            else if ( lazyLoader != null )
            {
                var impl = implFactory();
                ((IDomainObject)impl).SetLazyLoader(lazyLoader);
                return impl;
            }
            else
            {
                return null;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static IList<TContract> ImportDomainLazyLoadObjectCollection<TContract, TImpl>(
            IEntityRepository entityRepo,
            object importValue,
            Func<TImpl> itemImplFactory,
            out IPersistableObjectCollectionLazyLoader lazyLoader)
            where TImpl : TContract
        {
            var persistedCollection = (importValue as IEnumerable<IPersistableObject>);
            lazyLoader = (importValue as IPersistableObjectCollectionLazyLoader);

            if ( persistedCollection != null )
            {
                var concrete = new List<TImpl>();

                concrete.AddRange(persistedCollection.Select(persistedItem =>
                {
                    var itemImpl = itemImplFactory();
                    if ( persistedItem != null )
                    {
                        ((IDomainObject)itemImpl).ImportValues(entityRepo, persistedItem.ExportValues(entityRepo));
                    }
                    else
                    {
                        ((IDomainObject)itemImpl).InitializeValues(idManuallyAssigned: false);
                    }
                    return itemImpl;
                }));

                return new ConcreteToAbstractListAdapter<TImpl, TContract>(concrete);
            }
            else
            {
                return null;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static TPersistable ImportEmbeddedPersistableObject<TPersistable>(IEntityRepository entityRepo, object importValue)
            where TPersistable : class, IPersistableObject, new()
        {
            var domain = ((IPersistableObject)importValue);
            if ( domain != null )
            {
                var persistable = new TPersistable();
                persistable.ImportValues(entityRepo, domain.ExportValues(entityRepo));
                return persistable;
            }
            else
            {
                return null;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static TPersistable[] ImportEmbeddedPersistableCollection<TPersistable>(IEntityRepository entityRepo, object importValue)
            where TPersistable : class, IPersistableObject, new()
        {
            var domainCollection = (System.Collections.IEnumerable)importValue;

            if ( domainCollection == null )
            {
                return null;
            }

            var persistableCollection = domainCollection.Cast<IDomainObject>().Select(domainItem =>
            {
                if ( domainItem != null )
                {
                    var persistable = new TPersistable();
                    persistable.ImportValues(entityRepo, domainItem.ExportValues(entityRepo));
                    return persistable;
                }
                else
                {
                    return null;
                }
            }).ToArray();

            return persistableCollection;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static T EntityIdPropertyGetter<T>(
            IDomainObject target,
            ref IPersistableObjectLazyLoader targetLazyLoader,
            ref T backingField)
        {
            if ( targetLazyLoader != null )
            {
                var entityIdValue = targetLazyLoader.EntityId;

                if ( entityIdValue != null )
                {
                    return (T)entityIdValue;
                }

                targetLazyLoader.Load(target);
                targetLazyLoader = null;
            }

            return backingField;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static T PropertyGetter<T>(
            IDomainObject target,
            ref IPersistableObjectLazyLoader targetLazyLoader,
            ref T backingField)
        {
            if ( targetLazyLoader != null )
            {
                targetLazyLoader.Load(target);
                targetLazyLoader = null;
            }

            return backingField;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static T PropertySetter<T>(
            IDomainObject target,
            ref IPersistableObjectLazyLoader targetLazyLoader,
            out T backingField,
            ref T value)
        {
            if ( targetLazyLoader != null )
            {
                targetLazyLoader.Load(target);
                targetLazyLoader = null;
            }

            backingField = value;
            return value;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static T LazyLoadObjectPropertyGetter<T>(
            IDomainObject target,
            ref IPersistableObjectLazyLoader targetLazyLoader,
            ref IPersistableObjectLazyLoader propertyLazyLoader,
            ref T propertyBackingField)
        {
            if ( targetLazyLoader != null )
            {
                targetLazyLoader.Load(target);
                targetLazyLoader = null;
            }

            if ( propertyLazyLoader != null )
            {
                propertyBackingField = (T)propertyLazyLoader.Load((IDomainObject)propertyBackingField);
                propertyLazyLoader = null;
            }

            return propertyBackingField;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static void LazyLoadObjectPropertySetter<T>(
            IDomainObject target,
            ref IPersistableObjectLazyLoader targetLazyLoader,
            out IPersistableObjectLazyLoader propertyLazyLoader,
            out T propertyBackingField,
            ref T value)
        {
            if ( targetLazyLoader != null )
            {
                targetLazyLoader.Load(target);
                targetLazyLoader = null;
            }

            propertyLazyLoader = null;
            propertyBackingField = value;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static IList<TContract> LazyLoadObjectCollectionPropertyGetter<TContract, TDomainImpl>(
            IDomainObject target,
            ref IPersistableObjectLazyLoader targetLazyLoader,
            ref IPersistableObjectCollectionLazyLoader propertyLazyLoader,
            ref IList<TContract> propertyBackingField)
            where TDomainImpl : TContract
        {
            if ( targetLazyLoader != null )
            {
                targetLazyLoader.Load(target);
                targetLazyLoader = null;
            }

            if ( propertyLazyLoader != null )
            {
                propertyBackingField = new ConcreteToAbstractListAdapter<TDomainImpl, TContract>(propertyLazyLoader.Load().Cast<TDomainImpl>().ToList());
                propertyLazyLoader = null;
            }

            return propertyBackingField;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static IList<TContract> LazyLoadObjectCollection<TContract, TImpl>(
            IEntityRepository entityRepo,
            Func<TImpl> itemImplFactory,
            ref IPersistableObjectCollectionLazyLoader lazyLoader)
            where TImpl : TContract
        {
            var persistedCollection = lazyLoader.Load();
            var concrete = new List<TImpl>();

            if ( persistedCollection != null )
            {
                concrete.AddRange(persistedCollection.Select(persistedItem =>
                {
                    var itemImpl = itemImplFactory();
                    ((IDomainObject)itemImpl).ImportValues(entityRepo, persistedItem.ExportValues(entityRepo));
                    return itemImpl;
                }));
            }

            lazyLoader = null;
            return new ConcreteToAbstractListAdapter<TImpl, TContract>(concrete);
        }
        
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPersistableObjectLazyLoader
    {
        IDomainObject Load(IDomainObject target);
        Type EntityContractType { get; }
        Type DomainContextType { get; }
        object EntityId { get; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPersistableObjectCollectionLazyLoader
    {
        IEnumerable<IDomainObject> Load();
        Type EntityContractType { get; }
        Type DomainContextType { get; }
    }
}
