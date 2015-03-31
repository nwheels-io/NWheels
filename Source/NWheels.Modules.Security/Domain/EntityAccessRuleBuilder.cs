#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Modules.Security.Domain
{
    internal class EntityAccessRuleBuilder : IEntityAccessControlBuilder
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly Dictionary<Type, IEntityAccessRule> _accessRuleByContractType;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessRuleBuilder(ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            _accessRuleByContractType = new Dictionary<Type, IEntityAccessRule>();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReadOnly<TEntity>()
        {
            _accessRuleByContractType[typeof(TEntity)] = new ReadOnlyEntityAccessRule<TEntity>(_metadataCache.GetTypeMetadata(typeof(TEntity)));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Denied<TEntity>()
        {
            _accessRuleByContractType[typeof(TEntity)] = new DeniedEntityAccessRule<TEntity>(_metadataCache.GetTypeMetadata(typeof(TEntity)));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Filtered<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
        {
            _accessRuleByContractType[typeof(TEntity)] = new FilteredEntityAccessRule<TEntity>(_metadataCache.GetTypeMetadata(typeof(TEntity)), filter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Custom<TEntity>(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
            Func<TEntity, bool> canInsert,
            Func<TEntity, bool> canUpdate,
            Func<TEntity, bool> canDelete)
        {
            _accessRuleByContractType[typeof(TEntity)] = new CustomEntityAccessRule<TEntity>(
                _metadataCache.GetTypeMetadata(typeof(TEntity)), 
                filter,
                canInsert,
                canUpdate,
                canDelete);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Dictionary<Type, IEntityAccessRule> GetEntityAcccessRules()
        {
            return new Dictionary<Type, IEntityAccessRule>(_accessRuleByContractType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CustomEntityAccessRule<TEntity> : IEntityAccessRule<TEntity>
        {
            private readonly ITypeMetadata _metadata;
            private readonly Func<IQueryable<TEntity>, IQueryable<TEntity>> _filter;
            private readonly Func<TEntity, bool> _canInsert;
            private readonly Func<TEntity, bool> _canUpdate;
            private readonly Func<TEntity, bool> _canDelete;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CustomEntityAccessRule(
                ITypeMetadata metadata,
                Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
                Func<TEntity, bool> canInsert,
                Func<TEntity, bool> canUpdate,
                Func<TEntity, bool> canDelete)
            {
                _metadata = metadata;
                _canDelete = canDelete;
                _canUpdate = canUpdate;
                _canInsert = canInsert;
                _filter = filter;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable<TEntity> Filter(IQueryable<TEntity> source)
            {
                return _filter(source);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanInsert(TEntity entity)
            {
                return _canInsert(entity);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanUpdate(TEntity entity)
            {
                return _canUpdate(entity);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanDelete(TEntity entity)
            {
                return _canDelete(entity);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata GetTypeMetadata()
            {
                return _metadata;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanQuery()
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanInsert()
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanUpdate()
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanDelete()
            {
                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ReadOnlyEntityAccessRule<TEntity> : IEntityAccessRule<TEntity>
        {
            private readonly ITypeMetadata _metadata;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ReadOnlyEntityAccessRule(ITypeMetadata metadata)
            {
                _metadata = metadata;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable<TEntity> Filter(IQueryable<TEntity> source)
            {
                return source;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanInsert(TEntity entity)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanUpdate(TEntity entity)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanDelete(TEntity entity)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata GetTypeMetadata()
            {
                return _metadata;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanQuery()
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanInsert()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanUpdate()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanDelete()
            {
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class FilteredEntityAccessRule<TEntity> : IEntityAccessRule<TEntity>
        {
            private readonly ITypeMetadata _metadata;
            private readonly Func<IQueryable<TEntity>, IQueryable<TEntity>> _filter;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FilteredEntityAccessRule(ITypeMetadata metadata, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
            {
                _metadata = metadata;
                _filter = filter;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable<TEntity> Filter(IQueryable<TEntity> source)
            {
                return _filter(source);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanInsert(TEntity entity)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanUpdate(TEntity entity)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanDelete(TEntity entity)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata GetTypeMetadata()
            {
                return _metadata;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanQuery()
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanInsert()
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanUpdate()
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanDelete()
            {
                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DeniedEntityAccessRule<TEntity> : IEntityAccessRule<TEntity>
        {
            private readonly ITypeMetadata _metadata;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DeniedEntityAccessRule(ITypeMetadata metadata)
            {
                _metadata = metadata;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable<TEntity> Filter(IQueryable<TEntity> source)
            {
                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanInsert(TEntity entity)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanUpdate(TEntity entity)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanDelete(TEntity entity)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata GetTypeMetadata()
            {
                return _metadata;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanQuery()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanInsert()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanUpdate()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool CanDelete()
            {
                return false;
            }
        }
    }
}

#endif