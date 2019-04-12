using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.RestApi.Model.Impl.Parsers;

namespace NWheels.UI.RestApi.Model
{
    public abstract class AnyBackendApiProxy
    {
    }
    
    [ModelParser(typeof(BackendApiProxyParser))]
    public class BackendApiProxy<TApi> : AnyBackendApiProxy
    {
        public BackendApiProxy(string url)
        {
            Url = url;
        }

        public string Url { get; }
        public TApi Api => default;

        public static implicit operator TApi(BackendApiProxy<TApi> proxy)
        {
            return default;
        }
    }
}
