using System.Collections.Generic;

namespace NWheels.RestApi.Api
{
    public interface IResourceRouter
    {
        bool TryGetResourceBinding(string uriPath, string protocol, out IResourceBinding binding);
        bool TryGetResourceBinding<TMessageContext>(string uriPath, string protocol, out IResourceBinding<TMessageContext> binding);
    }
}
