using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Domains.Security.Core;

namespace NWheels.Domains.Security
{
    public static class IdentityInfoExtensions
    {
        public static IUserAccountEntity GetUserAccount(this IIdentityInfo identityInfo)
        {
            return ((UserAccountIdentity)identityInfo).GetUserAccount();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TUserAccount GetUserAccount<TUserAccount>(this IIdentityInfo identityInfo) where TUserAccount : IUserAccountEntity
        {
            return (TUserAccount)((UserAccountIdentity)identityInfo).GetUserAccount();
        }
    }
}
