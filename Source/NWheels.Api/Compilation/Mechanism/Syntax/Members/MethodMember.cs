using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class MethodMember : MethodMemberBase
    {
        public MethodMember(
            MemberVisibility visibility, 
            MethodParameter returnValue, 
            string name, 
            params MethodParameter[] parameters)
            : this(visibility, MemberModifier.None, name, new MethodSignature() {  })
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodMember(
            MemberVisibility visibility,
            string name,
            MethodSignature signature)
            : this(visibility, MemberModifier.None, name, signature)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodMember(
            MemberVisibility visibility,
            MemberModifier modifiers,
            string name,
            MethodSignature signature)
        {
            this.Visibility = visibility;
            this.Modifier = modifiers;
            this.Name = name;
            this.Signature = signature;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodMember(MethodInfo clrBinding)
            : base(clrBinding)
        {
            this.Name = Name;
            this.Signature = new MethodSignature(clrBinding);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodInfo ClrBinding { get; set; }
    }
}
