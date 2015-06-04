using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ViewNameAttribute : Attribute
    {
        public ViewNameAttribute(string viewName)
        {
            this.ViewName = viewName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ViewName { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ViewNameAttribute FromType(Type type)
        {
            return type.GetCustomAttribute<ViewNameAttribute>();
        }
    }
}
