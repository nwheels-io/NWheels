using System.Linq;
using MetaPrograms.Members;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;
using static MetaPrograms.Fluent.Generator;

namespace NWheels.UI.Adapters.Web.Wix
{
    public class WixCorvidGenerator
    {
        public ModuleMember GenerateCorvidModule(
            TechnologyAdapterContext<WebAppMetadata> context,
            WebAppMetadata.PageItem page)
        {
            var hasBackend = page.Metadata.BackendApis.Any();
            
            return MODULE(new string[0], "corvid", () => {

                if (hasBackend)
                {
                    IMPORT.TUPLE("fetch", out var @fetch).FROM("wix-fetch");
                    INCLUDE("Web.Wix.Code.backend-client.js");
                }

                USE("$w").DOT("onReady").INVOKE(LAMBDA(() => {
                    USE("$w").INVOKE(ANY("#html1")).DOT("onMessage").INVOKE(LAMBDA(@event => {
                        USE("console").DOT("log").INVOKE(ANY($"got message!"), @event.DOT("data"));
                    }));
                }));
                
            });
        }
    }
}