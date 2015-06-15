using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public static class ViewModelPropertyContract
    {
        [AttributeUsage(AttributeTargets.Property)]
        public class PersistedOnUserMachineAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class UserIPAddress : Attribute
        {
        }
    }
}
