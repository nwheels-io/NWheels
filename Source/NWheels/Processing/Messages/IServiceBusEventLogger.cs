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
        ILogActivity DispatchingMessageObject(string messageObjectType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogActivity]
        ILogActivity InvokingActor(string actorType, string messageBodyType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        Exception ActorFailed(string actorType, string messageBodyType, Exception error);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        AggregateException ErrorsWhileHandlingMessage(string messageBodyType, AggregateException errors);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        void NoSubscribersFound(string messageBodyType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        Exception FailedToObtainActorInstance(string messageBodyType, Exception error);

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
