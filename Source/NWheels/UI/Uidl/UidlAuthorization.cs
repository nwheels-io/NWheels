using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Exceptions;

namespace NWheels.UI.Uidl
{
    public class UidlAuthorization
    {
        public UidlAuthorization()
        {
            this.RequiredClaims = new HashSet<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlAuthorization(IEnumerable<AuthorizationContract.AuthorizationAttribute> attributes)
            : this()
        {
            foreach ( var attribute in attributes.Where(a => a != null) )
            {
                if ( attribute.UserRoles != null )
                {
                    this.RequiredClaims.UnionWith(attribute.UserRoles);
                }

                if ( attribute.Permissions != null )
                {
                    this.RequiredClaims.UnionWith(attribute.Permissions);
                }

                if ( attribute.EntityAccessRules != null )
                {
                    this.RequiredClaims.UnionWith(attribute.EntityAccessRules);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ValidateUser(IIdentityInfo identity)
        {
            if ( !TryValidateUser(identity) )
            { 
                throw new SecurityException("User is not authorized to perform requested operation.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryValidateUser(IIdentityInfo identity)
        {
            if ( this.RequiredClaims.Count == 0 )
            {
                return true;
            }

            foreach ( var claim in this.RequiredClaims )
            {
                if ( identity.GetAccessControlList().HasClaim(claim) )
                {
                    return true;
                }
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public HashSet<string> RequiredClaims { get; private set; }
    }
}
