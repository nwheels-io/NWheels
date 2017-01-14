using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class FieldMember : AbstractMember
    {
        public TypeMember FieldType { get; set; }
        public FieldInfo Binding { get; set; }
    }
}
