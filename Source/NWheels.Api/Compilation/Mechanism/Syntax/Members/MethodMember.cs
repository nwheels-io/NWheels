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
            : this(visibility, MemberModifiers.None, name, new MethodSignature() {  })
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodMember(
            MemberVisibility visibility,
            string name,
            MethodSignature signature)
            : this(visibility, MemberModifiers.None, name, signature)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodMember(
            MemberVisibility visibility,
            MemberModifiers modifiers,
            string name,
            MethodSignature signature)
            : base()
        {
            this.Visibility = visibility;
            this.Modifiers = modifiers;
            this.Name = Name;
            this.Signature = signature;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodMember(MethodInfo binding)
            : base(binding)
        {
            this.Name = Name;
            this.Signature = new MethodSignature(binding);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodInfo Bidning { get; set; }
    }
}
