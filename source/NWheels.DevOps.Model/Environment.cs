using NWheels.Composition.Model;
using NWheels.Composition.Model.Metadata;

namespace NWheels.DevOps.Model
{
    [ModelElement]
    public abstract class AnyEnvironment : ICanInclude<AnyDeployment>, ICanInclude<TechnologyAdaptedComponent>
    {
    }

    public abstract class Environment<TConfig> : AnyEnvironment
    {
        protected Environment(string name, string role, TConfig config)
        {
            this.Name = name;
            this.Role = role;
            this.Config = config;
        }

        public string Name { get; }
        public string Role { get; }
        public TConfig Config { get; }
    }
}
