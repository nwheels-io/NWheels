using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Members
{
    public interface IPropertyMember : IMember
    {
        IMethodMember Getter { get; }
        IMethodMember Setter { get; }
    }
}
