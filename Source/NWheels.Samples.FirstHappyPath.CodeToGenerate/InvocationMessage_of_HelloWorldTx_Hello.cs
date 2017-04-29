using NWheels.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using NWheels.Samples.FirstHappyPath.Domain;

namespace NWheels.Samples.FirstHappyPath.CodeToGenerate
{
    public class InvocationMessage_of_HelloWorldTx_Hello : TaskCompletionSource<object>, IInvocationMessage
    {
        private string _result;

        Task<object> IInvocationMessage.Awaitable => base.Task;

        Type IInvocationMessage.TargetType => typeof(HelloWorldTx);

        MethodInfo IInvocationMessage.TargetMethod => throw new NotImplementedException("Requires netstandard2.0"); //Module.ResolveMethod

        object IInvocationMessage.Result => _result;

        Exception IInvocationMessage.Exception => base.Task.Exception;

        TaskAwaiter<object> IInvocationMessage.GetAwaiter()
        {
            return base.Task.GetAwaiter();
        }

        async Task<object> IInvocationMessage.Invoke(object target)
        {
            _result = await ((HelloWorldTx)target).Hello(this.Name);
            return _result;
        }

        public string Name { get; set; }
        public string Result => _result;
    }
}
