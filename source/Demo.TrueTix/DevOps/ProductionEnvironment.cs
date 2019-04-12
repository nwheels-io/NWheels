using NWheels.Composition.Model;
using NWheels.DevOps.Model;

namespace Demo.TrueTix.DevOps
{
    public class ProductionEnvironment : Environment<EmptyConfiguration>
    {
        public ProductionEnvironment() 
            : base("prod", "prod", new EmptyConfiguration())
        {
        }
    }
}