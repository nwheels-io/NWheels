using System.Runtime.InteropServices;
using System.Xml.Linq;
using MetaPrograms;
using Newtonsoft.Json.Serialization;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Web;

namespace NWheels.UI.Adapters.Web.StaticHtml
{
    public class StaticHtmlTechnologyAdapter : ITechnologyAdapter
    {
        public void GenerateOutputs(ITechnologyAdapterContext context)
        {
            var html = new XElement("html",
                new XElement("head",
                    new XElement("title", "Hello World")
                ),
                new XElement("body",
                    new XElement("h1", "Hello, world!")
                )
            );

            var staticFolderPath = new FilePath("frontend", "hello-world-site", "static");
            var indexPagePath = staticFolderPath.Append("index.html");
            
            context.Output.AddSourceFile(indexPagePath, html.ToString());
            context.DeploymentScript.AddImage(BuildSiteImage());

            DeploymentImageMetadata BuildSiteImage()
            {
                return new DeploymentImageMetadata(context) {
                    Name = "hello-world-site",
                    BuildContextPath = staticFolderPath.Up(1),
                    BaseImage = "nginx",
                    FilesToCopy = {
                        {staticFolderPath.Tail(1), new FilePath("/usr", "share", "nginx", "html")}
                    }
                };
            }
        }
    }
}
