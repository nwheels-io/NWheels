using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using NWheels.Authorization.Claims;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Extensions;

namespace NWheels.Authorization.Impl
{
    internal class AccessControlList : IAccessControlList
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IAuthorizationLogger _logger;
        private readonly Claim[] _claims;
        private readonly string _claimSetKey;
        private readonly Dictionary<Type, IEntityAccessControl> _entityAccessControlByContract;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AccessControlList(ITypeMetadataCache metadataCache, IAuthorizationLogger logger, IEnumerable<Claim> claims, string claimSetKey)
        {
            _metadataCache = metadataCache;
            _logger = logger;
            _claims = claims.ToArray();
            _claimSetKey = claimSetKey;
            _entityAccessControlByContract = new Dictionary<Type, IEntityAccessControl>();

            BuildAccessControls(claims.OfType<EntityAccessRuleClaim>().Select(claim => claim.Rule));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityAccessControl<TEntity> GetEntityAccessControl<TEntity>()
        {
            IEntityAccessControl control;

            if ( _entityAccessControlByContract.TryGetValue(typeof(TEntity), out control) )
            {
                return (IEntityAccessControl<TEntity>)control;
            }

            throw _logger.NoRuleDefinedForEntity(contract: typeof(TEntity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyCollection<Claim> GetClaims()
        {
            return _claims;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override string ToString()
        {
            return _claimSetKey ?? base.ToString();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void BuildAccessControls(IEnumerable<IEntityAccessRule> entityAccessRules)
        {
            var builder = new EntityAccessControlBuilder(_metadataCache);

            foreach ( var rule in entityAccessRules )
            {
                builder.AddRule(rule);
            }

            builder.FillAccessControlsByContract(_entityAccessControlByContract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EntityAccessControlBuilder : IEntityAccessControlBuilder
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly Dictionary<Type, List<IEntityAccessControl>> _controlListByContract;
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityAccessControlBuilder(ITypeMetadataCache metadataCache)
            {
                _metadataCache = metadataCache;
                _controlListByContract = new Dictionary<Type, List<IEntityAccessControl>>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEntityAccessControlBuilder<TEntity> IEntityAccessControlBuilder.ToEntity<TEntity>()
            {
                var metaType = _metadataCache.GetTypeMetadata(typeof(TEntity));
                var control = new EntityAccessControl<TEntity>(metaType);

                RegisterControlForTypeAndInheritors(metaType, control);

                return control;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddRule(IEntityAccessRule rule)
            {
                rule.BuildAccessControl(this);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void FillAccessControlsByContract(Dictionary<Type, IEntityAccessControl> accessControlByContract)
            {
                foreach ( var contractEntry in _controlListByContract )
                {
                    if ( contractEntry.Value.Count == 1 )
                    {
                        accessControlByContract.Add(contractEntry.Key, contractEntry.Value[0]);
                    }
                    else if ( contractEntry.Value.Count > 1 )
                    {
                        contractEntry.Value.Sort((x, y) => y.MetaType.InheritanceDepth.CompareTo(x.MetaType.InheritanceDepth));
                        var controlPipe = EntityAccessControlPipe.Create(contractType: contractEntry.Key, sinks: contractEntry.Value);
                        accessControlByContract.Add(contractEntry.Key, controlPipe);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RegisterControlForTypeAndInheritors<TEntity>(ITypeMetadata metaType, EntityAccessControl<TEntity> control)
            {
                for ( var affectedMetaType = metaType ; affectedMetaType != null ; affectedMetaType = affectedMetaType.BaseType )
                {
                    var controlList = _controlListByContract.GetOrAdd(affectedMetaType.ContractType, key => new List<IEntityAccessControl>());
                    controlList.Add(control);
                }
            }
        }
    }
}
