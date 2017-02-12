using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class PropertyMember : AbstractMember
    {
        public PropertyMember()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMember(TypeMember declaringType, MemberVisibility visibility, MemberModifier modifier, TypeMember type, string name)
            : base(declaringType, visibility, modifier, name)
        {
            this.PropertyType = type;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember PropertyType { get; set; }
        public MethodMember Getter { get; set; }
        public MethodMember Setter { get; set; }
        public PropertyInfo PropertyBinding { get; set; }
    }
}
