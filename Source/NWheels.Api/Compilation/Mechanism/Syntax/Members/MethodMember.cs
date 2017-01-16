using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class MethodMember : MethodMemberBase
    {
        public MethodMember(MemberVisibility visibility, MethodParameter returnType, string Name, params MethodParameter[] parameters)
        {
            this.Visibility = visibility;
            this.Name = Name;
            this.Signature = new MethodSignature();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodInfo Bidning { get; set; }
    }
}
