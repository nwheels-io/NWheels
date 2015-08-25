using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

        [DataMember]
        public HashSet<string> RequiredClaims { get; private set; }
    }
}
