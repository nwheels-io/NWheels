namespace NWheels.Composition.Model
{
    public abstract class Package
    {
        public virtual string Description => this.GetType().FullName;
    }
    
    public abstract class Package<TConfig> : Package
        where TConfig : class, new()
    {
        protected Package()
            : this(new TConfig())
        {
        }

        protected Package(TConfig config)
        {
            this.Config = config;
        }
        
        public TConfig Config { get; }
    }

    public class AutomaticPackage : Package<EmptyConfiguration>
    {
    }

    public class EmptyConfiguration
    {
    }
   
}
