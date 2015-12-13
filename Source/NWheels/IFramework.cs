using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;
using NWheels.Entities;
using NWheels.Hosting;
using System.Data;
using NWheels.Authorization;

namespace NWheels
{
    public interface IFramework
    {
        T NewDomainObject<T>() where T : class;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        TRepository NewUnitOfWork<TRepository>(bool autoCommit = true, IsolationLevel? isolationLevel = null, string connectionString = null) 
            where TRepository : class, IApplicationDataRepository;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        IResourceLock NewLock(ResourceLockMode mode, string resourceNameFormat, params object[] formatArgs);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Guid NewGuid();
        int NewRandomInt32();
        long NewRandomInt64();
        INodeConfiguration CurrentNode { get; }
        IIdentityInfo CurrentIdentity { get; }
        string CurrentSessionId { get; }
        Guid CurrentCorrelationId { get; }
        DateTime UtcNow { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITimeoutHandle NewTimer(
            string timerName,
            string timerInstanceId,
            TimeSpan initialDueTime,
            Action callback);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITimeoutHandle NewTimer<TParam>(
            string timerName, 
            string timerInstanceId, 
            TimeSpan initialDueTime, 
            Action<TParam> callback, 
            TParam parameter);
    }
}
