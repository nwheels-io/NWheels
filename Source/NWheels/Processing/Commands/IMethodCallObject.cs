using System.Reflection;

namespace NWheels.Processing.Commands
{
    public interface IMethodCallObject
    {
        void ExecuteOn(object target);
        MethodInfo MethodInfo { get; }
    }
}
