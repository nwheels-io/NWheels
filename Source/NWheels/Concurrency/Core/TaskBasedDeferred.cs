using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency.Core
{
    public class TaskBasedDeferred : TaskCompletionSource<bool>, IDeferred, IAnyDeferred
    {
        public void Resolve()
        {
            base.SetResult(true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAnyDeferred.Resolve(object result)
        {
            base.SetResult(true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Fail(Exception error)
        {
            base.SetException(error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAnyDeferred.ResultType
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IAnyDeferred.Result
        {
            get
            {
                if (base.Task.Exception != null)
                {
                    throw base.Task.Exception;
                }
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure(
            Action continuation = null, 
            TimeSpan? timeout = null, 
            System.Threading.CancellationToken? cancellation = null)
        {
            if (continuation != null)
            {
                base.Task.ContinueWith(t => continuation);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsResolved
        {
            get { return base.Task.IsCompleted; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public Exception Error
        {
            get { return base.Task.Exception; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TaskBasedDeferred<T> : TaskCompletionSource<T>, IDeferred<T>, IAnyDeferred
    {
        public void Resolve(T result)
        {
            base.SetResult(result);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAnyDeferred.Resolve(object result)
        {
            var resultAsTask = (result as Task<T>);

            if (resultAsTask != null)
            {
                base.SetResult(resultAsTask.Result);
            }
            else
            {
                base.SetResult((T)result);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Fail(Exception error)
        {
            base.SetException(error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAnyDeferred.ResultType
        {
            get
            {
                return typeof(T);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IAnyDeferred.Result
        {
            get
            {
                return base.Task.Result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure(
            Action continuation = null,
            TimeSpan? timeout = null,
            System.Threading.CancellationToken? cancellation = null)
        {
            if (continuation != null)
            {
                base.Task.ContinueWith(t => continuation);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsResolved
        {
            get { return base.Task.IsCompleted; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T Result
        {
            get { return base.Task.Result; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception Error
        {
            get { return base.Task.Exception; }
        }
    }
}
