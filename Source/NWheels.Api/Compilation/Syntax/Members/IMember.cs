using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Members
{
    public interface IMember
    {
        string Name { get; }
        ITypeMember DeclaringType { get; }
        MemberStatus Status { get; }
        MemberVisibility Visibility { get; }
        MemberModifiers Modifiers { get; }
        MemberInfo MemberBinding { get; }
    }
}
