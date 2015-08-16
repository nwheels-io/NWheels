using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Processing.Messages
{
    public interface IServiceBusEventLogger : IApplicationEventLogger
    {
        [LogActivity]
        ILogActivity DispatchingMessage(string messageType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogActivity]
        ILogActivity InvokingActor(string actorType, string messageType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        Exception ActorFailed(string actorType, string messageType, Exception error);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        AggregateException ErrorsWhileHandlingMessage(string messageType, AggregateException errors);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        void NoSubscribersFound(string messageType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        Exception FailedToObtainActorInstance(string messageType, Exception error);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogWarning]
        void ServiceBusDidNotStopInTimelyFashion();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo]
        void WorkerThreadStarted();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo]
        void WorkerThreadStopped();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogDebug]
        void ListenerCanceled();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogCritical]
        void WorkerThreadTerminatedWithUnhandledException(Exception error);
    }
}
