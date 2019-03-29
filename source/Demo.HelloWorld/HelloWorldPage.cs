using NWheels.Composition.Model;
using NWheels.UI.Model;
using NWheels.UI.Model.Web;

namespace Demo.HelloWorld
{
    class HelloWorldPage : WebPage
    {
        [Include]
        UIComponent Hello => "Hello, world!"; 
    }
}
