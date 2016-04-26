using System.Reflection;

namespace NWheels.Processing.Commands
{
    public interface IMethodCallObject
    {
        //bool CheckAuthorization(out bool authenticationRequired);
        void ExecuteOn(object target);
        object GetParameterValue(int index);
        object GetParameterValue(string name);
        void SetParameterValue(int index, object value);
        void SetParameterValue(string name, object value);
        MethodInfo MethodInfo { get; }
        object Result { get; }
    }
}
