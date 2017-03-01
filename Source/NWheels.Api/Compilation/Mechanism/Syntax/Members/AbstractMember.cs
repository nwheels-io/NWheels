using NWheels.Extensions;
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

        protected AbstractMember(MemberVisibility visibility, MemberModifier modifier, string name)
            : this()
        {
            this.Visibility = visibility;
            this.Modifier = modifier;
            this.Name = name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractMember(TypeMember declaringType, MemberVisibility visibility, MemberModifier modifier, string name)
            : this(visibility, modifier, name)
        {
            this.DeclaringType = declaringType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractMember(MemberInfo clrBinding)
            : this()
        {
            this.Status = MemberStatus.Compiled;
            this.Name = clrBinding.Name;
            this.DeclaringType = clrBinding.DeclaringType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void AcceptVisitor(MemberVisitor visitor)
        {
            if (this.Attributes != null)
            {
                foreach (var attribute in this.Attributes)
                {
                    visitor.VisitAttribute(attribute);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return $"{this.GetType().Name.TrimSuffix("Member")} {this.Name}";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; set; }
        public TypeMember DeclaringType { get; set; }
        public MemberStatus Status { get; set; }
        public MemberVisibility Visibility { get; set; }
        public MemberModifier Modifier { get; set; }
        public List<AttributeDescription> Attributes { get; private set; }
    }
}
