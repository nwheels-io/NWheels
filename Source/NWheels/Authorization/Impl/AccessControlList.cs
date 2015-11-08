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
        private IEntityAccessControl _defaultControl;

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

        public IEntityAccessControl GetEntityAccessControl(Type entityContractType)
        {
            IEntityAccessControl controlForEntity;

            if ( _entityAccessControlByContract.TryGetValue(entityContractType, out controlForEntity) )
            {
                return controlForEntity;
            }
            else
            {
                return _defaultControl;
            }
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

            builder.FillEntityAccessControlDictionary(_entityAccessControlByContract, out _defaultControl);
        }
    }
}
