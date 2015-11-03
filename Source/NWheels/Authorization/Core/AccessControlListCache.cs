using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NWheels.Authorization.Claims;
using NWheels.Authorization.Impl;
using NWheels.DataObjects;
using System.Collections.Concurrent;
using System.Text;

namespace NWheels.Authorization.Core
{
    public class AccessControlListCache
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IAuthorizationLogger _logger;
        private readonly ConcurrentDictionary<string, IAccessControlList> _accessControlListByClaimSetKey;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AccessControlListCache(ITypeMetadataCache metadataCache, IAuthorizationLogger logger)
        {
            _metadataCache = metadataCache;
            _logger = logger;
            _accessControlListByClaimSetKey = new ConcurrentDictionary<string, IAccessControlList>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAccessControlList GetAccessControlList(Claim[] claimSet)
        {
            var key = CreateClaimSetKey(claimSet);
            
            var acl = _accessControlListByClaimSetKey.GetOrAdd(
                key,
                k => {
                    return new AccessControlList(_metadataCache, _logger, claimSet, claimSetKey: k);
                });

            return acl;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string CreateClaimSetKey(Claim[] claimSet)
        {
            var sortedClaims = new List<Claim>(claimSet);
            sortedClaims.Sort(CompareClaimsByTypeThenByValue);
            
            var key = new StringBuilder();

            for ( int i = 0 ; i < sortedClaims.Count ; i++ )
            {
                key.Append(sortedClaims[i].Type + '=' + sortedClaims[i].Value + ';');
            }

            return key.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static int CompareClaimsByTypeThenByValue(Claim a, Claim b)
        {
            var result = StringComparer.InvariantCultureIgnoreCase.Compare(a.Type, b.Type);

            if ( result != 0 )
            {
                return result;
            }

            return StringComparer.InvariantCultureIgnoreCase.Compare(a.Value, b.Value);
        }
    }
}
