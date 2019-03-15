namespace NWheels.Package.Model
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
        
        protected abstract void Contribute(IContributions contributions);
    }

    public abstract class AutomaticPackage : Package<EmptyConfiguration>
    {
        [MetaMember]
        protected override void Contribute(IContributions contributions)
        {            
        }
    }

    public class EmptyConfiguration
    {
    }
    
    public interface IContributions
    {
    }

    public static class PackageContributionsExtensions
    {
        public static void IncludePackage(this IContributions contributions, Package package)
        {
        }
    } 
}
