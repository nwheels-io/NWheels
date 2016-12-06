using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints.Core;

namespace NWheels.Endpoints
{
    #pragma warning disable 1998

    public class EndpointMessagePipeline
    {
        public async Task<object> sd()
        {
            var promise = new MessagePromise<object>();
            return await promise;
        }
    }
    
    #pragma warning restore 1998

    public abstract class EndpointMessagePipeline2 : EndpointMessagePipeline
    {
        public override async MessageAwaiter sd()
        {
            await Task.Delay(100);
            return await base.sd();
        }
    }


    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class MessagePromise<T>
    {
        public MessageAwaiter<T> GetAwaiter()
        {
            return new MessageAwaiter<T>();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct MessageAwaiter<T> : INotifyCompletion
    {
        #region Implementation of INotifyCompletion

        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        #endregion

        public bool IsCompleted
        {
            get { return false; }
        }

        public T GetResult()
        {
            return default(T);
        } 
    }
}
