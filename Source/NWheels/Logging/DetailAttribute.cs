using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class DetailAttribute : Attribute
    {
        public static bool IsDefinedOn(ParameterInfo parameter)
        {
            return (parameter.GetCustomAttribute<DetailAttribute>() != null);
        }
    }
}
