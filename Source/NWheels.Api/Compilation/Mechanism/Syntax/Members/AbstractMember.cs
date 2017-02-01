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
            this.Attributes = new List<AttributeDescription>();
            this.Status = MemberStatus.Generator;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractMember(MemberInfo binding)
            : this()
        {
            this.Status = MemberStatus.Compiled;
            this.Name = binding.Name;
            this.DeclaringType = binding.DeclaringType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; set; }
        public TypeMember DeclaringType { get; set; }
        public MemberStatus Status { get; set; }
        public MemberVisibility Visibility { get; set; }
        public MemberModifiers Modifiers { get; set; }
        public List<AttributeDescription> Attributes { get; private set; }
    }
}
