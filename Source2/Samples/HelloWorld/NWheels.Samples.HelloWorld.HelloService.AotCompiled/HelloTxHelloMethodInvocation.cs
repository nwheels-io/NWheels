using System;
using System.Reflection;
using System.Threading.Tasks;
using NWheels.Kernel.Api.Execution;
using NWheels.Samples.HelloWorld.HelloService;


namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    public class HelloTxHelloMethodInvocation : IInvocation
    {
        public InputStruct Input;
        public OutputStruct Output;
        
        public async Task Invoke(object target)
        {
            var typedTarget = (Program.HelloTx)target;
            Output.ReturnValue = await typedTarget.Hello(Input.Name);
        }

        Task IInvocation.Promise => throw new NotImplementedException();

        object IInvocation.Result => Output.ReturnValue;

        Type IInvocation.TargetType => typeof(Program.HelloTx);

        MethodInfo IInvocation.TargetMethod => throw new NotImplementedException();

        public struct InputStruct
        {
            public string Name;
        }

        public struct OutputStruct
        {
            public string ReturnValue;
        }
    }
}