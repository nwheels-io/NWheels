using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.RestApi.Model.Impl.Parsers;

namespace NWheels.UI.RestApi.Model
{
    [ModelParser(typeof(BackendApiProxyParser))]
    public abstract class AnyBackendApiProxy
    {
    }
    
    public class BackendApiProxy<TApi> : AnyBackendApiProxy
    {
        public BackendApiProxy(string url)
        {
            Url = url;
        }

        public string Url { get; }
        public TApi Api => default;
    }
}
