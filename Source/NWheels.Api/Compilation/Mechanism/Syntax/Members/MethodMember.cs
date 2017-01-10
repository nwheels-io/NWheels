using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public abstract class MethodMember : MethodMemberBase
    {
        public MethodInfo Bidning { get; set; }
    }
}
