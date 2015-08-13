using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;
using NWheels.Entities;
using NWheels.Hosting;
using System.Data;

namespace NWheels
{
    public interface IFramework
    {
        T New<T>() where T : class;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        TRepository NewUnitOfWork<TRepository>(bool autoCommit = true, IsolationLevel? isolationLevel = null) where TRepository : class, IApplicationDataRepository;
        IApplicationDataRepository NewUnitOfWork(Type repositoryContractType, bool autoCommit = true, IsolationLevel? isolationLevel = null);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        IResourceLock NewLock(ResourceLockMode mode, string resourceNameFormat, params object[] formatArgs);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Guid NewGuid();
        int NewRandomInt32();
        long NewRandomInt64();
        INodeConfiguration CurrentNode { get; }
        Guid CurrentCorrelationId { get; }
        DateTime UtcNow { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITimerHandle NewTimer(
            string timerName,
            string timerInstanceId,
            TimeSpan initialDueTime,
            TimeSpan? recurringPeriod,
            Action callback);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITimerHandle NewTimer<TParam>(
            string timerName, 
            string timerInstanceId, 
            TimeSpan initialDueTime, 
            TimeSpan? recurringPeriod, 
            Action<TParam> callback, 
            TParam parameter);
    }
}
