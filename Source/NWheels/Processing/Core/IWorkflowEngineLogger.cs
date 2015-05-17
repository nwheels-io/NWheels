using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Exceptions;
using NWheels.Logging;

namespace NWheels.Processing.Core
{
    public interface IWorkflowEngineLogger : IApplicationEventLogger
    {
        [LogError]
        RegistrationMissingException CodeBehindTypeNotRegistered(string codeBehindTypeName);

        [LogInfo]
        void InstanceStarted(Guid instanceId, string codeBehindTypeName);

        [LogInfo]
        void InstanceSuspended(Guid instanceId, string codeBehindTypeName);

        [LogInfo]
        void InstanceResumed(Guid instanceId, string codeBehindTypeName);

        [LogInfo]
        void InstanceCompleted(Guid instanceId, string codeBehindTypeName);

        [LogError]
        void InstanceFailed(Guid instanceId, string codeBehindTypeName, Exception error);

        [LogError]
        ArgumentException InstanceNotFound(Guid instanceId);

        [LogActivity]
        ILogActivity RecoveringExistingInstances();
    }
}
