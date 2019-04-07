using NWheels.Composition.Model;
using NWheels.Composition.Model.Impl;

namespace NWheels.DevOps.Model
{
    public abstract class AnyDeployment : ICanInclude<TechnologyAdaptedComponent>
    {
    }
    
    public abstract class Deployment<TEnvConfig> : AnyDeployment 
        where TEnvConfig : class, new()
    {
        protected Deployment()
            : this(new TEnvConfig())
        {
        }

        protected Deployment(TEnvConfig config)
        {
            this.EnvConfig = config;
        }

        public TEnvConfig EnvConfig { get; } 
    }
    
    public abstract class Deployment : Deployment<EmptyConfiguration>
    {
    }
}
