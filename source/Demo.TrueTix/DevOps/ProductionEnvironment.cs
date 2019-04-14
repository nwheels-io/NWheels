using Demo.TrueTix.FrontEnd;
using NWheels.Composition.Model;
using NWheels.DevOps.Model;
using NWheels.UI.Adapters.Web.ReactRedux;
using NWheels.UI.Adapters.Web.Wix;

namespace Demo.TrueTix.DevOps
{
    public class ProductionEnvironment : Environment<EnvironmentConfig>
    {
        public ProductionEnvironment() 
            : base("prod", "prod", new EnvironmentConfig())
        {
        }

        [Include] public WixWebSite BuyerApp => 
            new BuyerApp(Config.BackendUrl)
                .AsWixWebSite(backendUrl: "https://api.tixlab.app/api/graphql");
                //.AsReactReduxWebApp();
    }
}