using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public abstract class AbstractMember
    {
        protected AbstractMember()
        {
            this.Attributes = new List<AttributeInfo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; set; }
        public TypeMember DeclaringType { get; set; }
        public MemberStatus Status { get; set; }
        public MemberVisibility Visibility { get; set; }
        public MemberModifiers Modifiers { get; set; }
        public List<AttributeInfo> Attributes { get; private set; }
    }
}
