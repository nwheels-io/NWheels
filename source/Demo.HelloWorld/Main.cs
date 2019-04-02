using System.Collections.Generic;
using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;
using NWheels.DevOps.Model;
using NWheels.UI.Adapters.Web.StaticHtml;
using NWheels.UI.Model.Web;

namespace Demo.HelloWorld
{
    public class Main : SystemMain
    {
        [Include]
        GkeEnvironment Production => new ProductionEnvironment().AsGkeEnvironment(
            zone: "us-central",
            project: "nwheels-demos"
        );
    }

    public class ProductionEnvironment : Environment<EmptyConfiguration>
    {
        public ProductionEnvironment() 
            : base(name: "prod", role: "prod", config: new EmptyConfiguration())
        {
        }
        
        [Include]
        public StaticHtmlWebSite HelloWorldSite => new SinglePageWebApp<HelloWorldPage>().AsStaticHtmlWebSite();
    }
}
