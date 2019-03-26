using System;
using NWheels.Composition.Model;

namespace NWheels.DevOps.Model
{
    public abstract class Microservice<TConfig> : Package<TConfig>
        where TConfig : class, new()
    {
        protected Microservice(string name, TConfig config, MicroserviceOptions options)
            : base(config)
        {
            this.Name = name;
            this.StateOption = options.State;
            this.AvailabilityOption = options.Availability;
        }
        
        public string Name { get; }
        public StateOption StateOption { get; }
        public AvailabilityOption AvailabilityOption { get; }
    }

    public class MicroserviceOptions
    {
        public StateOption State { get; set; }
        public AvailabilityOption Availability { get; set; }
    }
    
    public enum StateOption
    {
        Stateless,
        Stateful
    }

    public enum AvailabilityOption
    {
        Normal,
        High1Nine,
        High2Nines,
        High3Nines,
        High4Nines,
        High5Nines,
        High6Nines,
        High7Nines,
        High8Nines,
        High9Nines
    }
}
