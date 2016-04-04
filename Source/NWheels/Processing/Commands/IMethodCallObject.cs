using System.Reflection;

namespace NWheels.Processing.Commands
{
    public interface IMethodCallObject
    {
        //bool CheckAuthorization(out bool authenticationRequired);
        void ExecuteOn(object target);
        object GetParameterValue(int index);
        void SetParameterValue(int index, object value);
        MethodInfo MethodInfo { get; }
        object Result { get; }
    }
}
