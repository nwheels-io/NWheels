using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;
using NWheels.UI.Adapters.Web.StaticHtml;
using NWheels.UI.Model.Web;

namespace Demo.HelloWorld
{
    public class ProductionEnvironment : GkeEnvironment<EmptyConfiguration>
    {
        public ProductionEnvironment() 
            : base(name: "prod", role: "prod", config: new EmptyConfiguration())
        {
        }
        
        [Include]
        public StaticHtmlWebSite HelloWorldSite => new SinglePageWebApp<HelloWorldPage>().AsStaticHtmlWebSite();
    }
}
