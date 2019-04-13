using Demo.TrueTix.FrontEnd;
using NWheels.Composition.Model;
using NWheels.DevOps.Model;
using NWheels.UI.Adapters.Web.ReactRedux;

namespace Demo.TrueTix.DevOps
{
    public class ProductionEnvironment : Environment<EnvironmentConfig>
    {
        public ProductionEnvironment() 
            : base("prod", "prod", new EnvironmentConfig())
        {
        }

        [Include] public ReactReduxWebApp BuyerApp => 
            new BuyerApp(Config.BackendUrl)
                .AsReactReduxWebApp();
    }
}