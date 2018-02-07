using System;
using System.Threading;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Microservices.Api
{
    public interface IMicroserviceHost : IDisposable
    {
        void Compile(CancellationToken cancellation);
        void Configure(CancellationToken cancellation);
        void RunBatchJob(Action batchJob, CancellationToken cancellation, TimeSpan stopTimeout, out bool stoppedWithinTimeout);
        void RunDaemon(CancellationToken cancellation, TimeSpan stopTimeout, out bool stoppedWithinTimeout);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IComponentContainer GetBootComponents();
        IComponentContainer GetModuleComponents();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IBootConfiguration BootConfig { get; }
        MicroserviceState CurrentState { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        event EventHandler CurrentStateChanged;
    }
}