using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FormatAttribute : Attribute
    {
        public FormatAttribute(string formatString)
        {
            this.FormatString = formatString;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FormatString { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetFormatString(ParameterInfo parameter)
        {
            var attribute = parameter.GetCustomAttribute<FormatAttribute>();
            return (attribute != null ? attribute.FormatString : null);
        }
    }
}
