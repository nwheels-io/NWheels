using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class UITemplateAttribute : Attribute
    {
        public UITemplateAttribute(string templateName)
        {
            this.TemplateName = templateName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string TemplateName { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UITemplateAttribute FromType(Type type)
        {
            return type.GetCustomAttribute<UITemplateAttribute>();
        }
    }
}
