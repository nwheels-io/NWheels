using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiredClaimsAttribute : Attribute
    {
        public RequiredClaimsAttribute(params string[] claims)
        {
            this.Claims = claims;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] Claims { get; private set; }
    }
}
