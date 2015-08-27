using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Commands.Impl;

namespace NWheels.Processing.Commands.Factories
{
    public interface IMethodCallObjectFactory
    {
        Type GetMessageCallObjectType(MethodInfo method);
        IMethodCallObject NewMessageCallObject(MethodInfo method);
    }
}
