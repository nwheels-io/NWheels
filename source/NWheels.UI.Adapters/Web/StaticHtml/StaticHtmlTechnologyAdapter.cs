using System.Xml.Linq;
using Newtonsoft.Json.Serialization;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Web;

namespace NWheels.UI.Adapters.Web.StaticHtml
{
    public class StaticHtmlTechnologyAdapter : ITechnologyAdapter
    {
        public void Execute(ITechnologyAdapterContext context)
        {
            var html = new XElement("html",
                new XElement("head",
                    new XElement("title", "Hello World")
                ),
                new XElement("body",
                    new XElement("h1", "Hello, world!")
                )
            );
            
            context.Output.AddSourceFile(
                new[] { "frontend", "hello-world-site" },
                "index.html",
                html.ToString()
            );
        }
    }
}