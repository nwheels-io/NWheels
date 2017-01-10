using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class EventMember : AbstractMember
    {
        public TypeMember DelegateType { get; set; }
        public MethodMember Adder { get; set; }
        public MethodMember Remover { get; set; }
        public EventInfo Binding { get; set; }
    }
}
