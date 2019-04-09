using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using MetaPrograms;
using MetaPrograms.Extensions;
using Microsoft.CodeAnalysis.VisualBasic;
using Newtonsoft.Json.Serialization;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;
using NWheels.UI.Model.Web;

namespace NWheels.UI.Adapters.Web.StaticHtml
{
    public class StaticHtmlTechnologyAdapter : ITechnologyAdapter
    {
        public void GenerateOutputs(ITechnologyAdapterContext context)
        {
            var appMeta = (WebAppMetadata) context.Input;
            var appName = appMeta.Header.Name.TrimSuffixFragment("App");
            var appNameWithSiteSuffix = appName.AppendSuffixFragments("Site");
            var appTitle = appMeta.Title ?? appName.ToString(CasingStyle.Kebab); //TODO: CasingStyle.Human
            var staticFolderPath = new FilePath("frontend", appNameWithSiteSuffix.ToString(CasingStyle.Kebab), "static");

            foreach (var page in appMeta.Pages)
            {
                var html = GeneratePageHtml(page);
                var fileName = page.IsIndex ? "index" : page.Name.ToString(CasingStyle.Kebab);
                context.Output.AddSourceFile(staticFolderPath.Append($"{fileName}.html"), html.ToString());
            }
            
            context.DeploymentScript.AddImage(BuildDeploymentImage());

            DeploymentImageMetadata BuildDeploymentImage()
            {
                var publicEndpoint = context.Adapter.Parameters["publicEndpoint"] as string;
                
                return new DeploymentImageMetadata(context) {
                    Name = appNameWithSiteSuffix.ToString(CasingStyle.Kebab),
                    BuildContextPath = staticFolderPath.Up(1),
                    BaseImage = "nginx",
                    FilesToCopy = {
                        {staticFolderPath.Tail(1), new FilePath("/usr", "share", "nginx", "html")}
                    },
                    PublicEndpoint = publicEndpoint
                };
            }
            
            XElement GeneratePageHtml(WebAppMetadata.PageItem page)
            {
                var componentHtmls = page.Metadata.Components.Select(GenerateComponentHtml);
                var pageTitle = page.IsIndex 
                    ? appTitle 
                    : page.Name.ToString(CasingStyle.Kebab); //TODO: CasingStyle.Human 
            
                var html = new XElement("html",
                    new XElement("head",
                        new XElement("title", pageTitle)
                    ),
                    new XElement("body", 
                        componentHtmls.Cast<object>().ToArray()
                    )
                );

                return html;
            }

            IEnumerable<XElement> GenerateComponentHtml(UIComponentMetadata compMeta)
            {
                if (compMeta is TextContentMetadata textContent)
                {
                    yield return new XElement("h1", textContent.Text);
                }
            }
        }
    }
}
