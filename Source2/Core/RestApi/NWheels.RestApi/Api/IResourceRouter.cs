using System.Collections.Generic;

namespace NWheels.RestApi.Api
{
    public interface IResourceRouter
    {
        IEnumerable<IResourceHandler> GetAllResourceHandlers();
        bool TryGetResourceHandler(string fullPath, out IResourceHandler handler);
        IEnumerable<IResourceBinding> GetResourceBindings(IResourceHandler handler);
        bool TryGetResourceBinding(IResourceHandler handler, string protocol, out IResourceBinding binding);
        bool TryGetResourceBinding<TMessageContext>(
            IResourceHandler handler, string protocol, out IResourceBinding<TMessageContext> binding);
    }
}
