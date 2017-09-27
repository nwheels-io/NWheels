using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices.Runtime
{
    public class MicroserviceStateMachineOptions
    {
        public MicroserviceHost Host { get; set; }
        public IBootConfiguration BootConfig { get; set; }
        public Func<MicroserviceTrigger> OnConfiguring { get; set; }
        public Func<MicroserviceTrigger> OnCompiling { get; set; }
        public Action OnCompiledStopped { get; set; }
        public Func<MicroserviceTrigger> OnLoading { get; set; }
        public Func<MicroserviceTrigger> OnActivating { get; set; }
        public Func<MicroserviceTrigger> OnDeactivating { get; set; }
        public Func<MicroserviceTrigger> OnUnloading { get; set; }
        public Action OnFaulted { get; set; }
    }
}
