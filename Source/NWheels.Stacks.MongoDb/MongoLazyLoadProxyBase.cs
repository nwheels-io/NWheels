using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.DataObjects.Core;
using NWheels.Entities.Factories;
using NWheels.TypeModel.Core;
using System.Reflection;
using NWheels.Extensions;

namespace NWheels.Stacks.MongoDb
{
    public abstract class MongoLazyLoadProxyBase<TContract, TId> : IEntityObject, IHaveDependencies
        where TContract : class
    {
        private TId _id;
        private IComponentContext _components;
        private TContract _real;
        private IDomainObject _container;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MongoLazyLoadProxyBase(TId id)
        {
            _id = id;
            _real = null;
            _container = null;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IObject

        Type IObject.ContractType
        {
            get { return typeof(TContract); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IObject.FactoryType
        {
            get { return typeof(MongoEntityObjectFactory); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IObject.IsModified
        {
            get
            {
                if ( _real != null )
                {
                    return ((IEntityObject)_real).IsModified;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityObjectBase

        EntityState IEntityObjectBase.State
        {
            get
            {
                if ( _real != null )
                {
                    return ((IEntityObjectBase)_real).State;
                }
                else
                {
                    return EntityState.RetrievedPristine;
                }
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IContainedIn<out IDomainObject>

        IDomainObject IContainedIn<IDomainObject>.GetContainerObject()
        {
            return _container;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IPersistableObject

        void IPersistableObject.SetContainerObject(IDomainObject container)
        {
            _container = container;

            if ( _real != null )
            {
                ((IPersistableObject)_real).SetContainerObject(container);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IPersistableObject.EnsureDomainObject()
        {
            RuntimeEntityModelHelpers.EnsureContainerDomainObject<TContract>(this, _components);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityObject

        IEntityId IEntityObject.GetId()
        {
            if ( _real != null )
            {
                return ((IEntityObject)_real).GetId();
            }
            else
            {
                return new EntityId<TContract, TId>(_id);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityObject.SetId(object value)
        {
            _id = (TId)value;

            if ( _real != null )
            {
                ((IEntityObject)_real).SetId(value);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IHaveDependencies

        void IHaveDependencies.InjectDependencies(IComponentContext components)
        {
            _components = components;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TContract MongoLazyLoadProxyBaseGetReal()
        {
            if ( _real == null )
            {
                var persistable = MongoDataRepositoryBase.Current.LazyLoadById<TContract, TId>(_id).As<IPersistableObject>();
                persistable.SetContainerObject(_container);
                _real = (TContract)persistable;
            }

            return _real;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TId MongoLazyLoadProxyBaseGetId()
        {
            return _id;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Expression<Func<MongoLazyLoadProxyBase<TContract, TId>, TId>> _s_getIdMethodLambda = 
            x => x.MongoLazyLoadProxyBaseGetId();
        
        private static readonly Expression<Func<MongoLazyLoadProxyBase<TContract, TId>, TContract>> _s_getRealMethodLambda = 
            x => x.MongoLazyLoadProxyBaseGetReal();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MethodInfo MongoLazyLoadProxyBaseGetIdMethodInfo
        {
            get
            {
                return _s_getIdMethodLambda.GetMethodInfo();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MethodInfo MongoLazyLoadProxyBaseGetRealMethodInfo
        {
            get
            {
                return _s_getRealMethodLambda.GetMethodInfo();
            }
        }
    }
}
