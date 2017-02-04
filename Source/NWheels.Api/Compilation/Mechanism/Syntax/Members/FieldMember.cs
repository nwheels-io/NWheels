using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class FieldMember : AbstractMember
    {
        public FieldMember()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FieldMember(TypeMember declaringType, MemberVisibility visibility, MemberModifier modifier, TypeMember type, string name)
            : base(declaringType, visibility, modifier, name)
        {
            this.Type = type;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FieldMember(FieldInfo clrBinding)
            : base(clrBinding)
        {
            this.ClrBinding = clrBinding;
            this.Type = clrBinding.FieldType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember Type { get; set; }
        public FieldInfo ClrBinding { get; set; }
    }
}
