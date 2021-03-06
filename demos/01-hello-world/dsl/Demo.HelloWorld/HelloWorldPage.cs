using NWheels.Composition.Model;
using NWheels.UI.Model;
using NWheels.UI.Model.Web;

namespace Demo.HelloWorld
{
    class HelloWorldPage : WebPage
    {
        public override string Title => "NWheels Demo";

        [Include]
        UIComponent Hello => new TextContent("Hello, world!"); 
        
        // TODO: use implicit conversion:
        // UIComponent Hello => "Hello, world!"; 
    }

//    class HelloWorldApp : SinglePageWebApp<HelloWorldPage>
//    {
//    }
}
