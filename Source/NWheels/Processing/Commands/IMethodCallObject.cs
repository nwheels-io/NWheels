using System.Reflection;

namespace NWheels.Processing.Commands
{
    public interface IMethodCallObject
    {
        void ExecuteOn(object target);
        object GetParameterValue(int index);
        MethodInfo MethodInfo { get; }
        object Result { get; }
    }
}
