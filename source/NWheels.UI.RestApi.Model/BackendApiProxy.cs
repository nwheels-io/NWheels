namespace NWheels.UI.RestApi.Model
{
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
