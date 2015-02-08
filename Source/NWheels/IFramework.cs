using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;
using NWheels.Entities;
using NWheels.Hosting;

namespace NWheels
{
    public interface IFramework
    {
        TRepository NewUnitOfWork<TRepository>(bool autoCommit = true) where TRepository : class, IApplicationDataRepository;
        IResourceLock NewLock(ResourceLockMode mode, string resourceNameFormat, params object[] formatArgs);
        Guid NewGuid();
        int NewRandomInt32();
        long NewRandomInt64();
        INodeConfiguration CurrentNode { get; }
        Guid CurrentCorrelationId { get; }
        DateTime UtcNow { get; }
    }
}
