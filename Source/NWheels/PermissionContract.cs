using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels
{
    public static class PermissionContract
    {
        [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
        public class AnonymousAttribute : Attribute
        {
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
        public class RequireAttribute : Attribute
        {
            public RequireAttribute(object permission)
            {
                this.Permission = permission;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public object Permission { get; private set; }
        }
    }
}
