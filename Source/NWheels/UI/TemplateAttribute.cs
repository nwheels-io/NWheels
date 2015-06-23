using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class TemplateAttribute : Attribute
    {
        public TemplateAttribute(string templateName)
        {
            this.TemplateName = templateName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string TemplateName { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ApplyIfDefined(MemberInfo member, WidgetUidlNode widget)
        {
            var attribute = member.GetCustomAttribute<TemplateAttribute>();

            if ( attribute != null )
            {
                widget.TemplateName = attribute.TemplateName;
            }
        }
    }
}
