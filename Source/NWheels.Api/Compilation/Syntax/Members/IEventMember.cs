using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Members
{
    public interface IEventMember : IMember
    {
        IMethodMember Adder { get; }
        IMethodMember Remover { get; }
    }
}
