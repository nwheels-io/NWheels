using System.Threading.Tasks;

namespace NWheels.RestApi.Api
{
    public interface IResourceBinding
    {
        IResourceDescription Resource { get; }
        string Protocol { get; }
        string UriPath { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IResourceBinding<in TMessageContext> : IResourceBinding
    {
        Task HandleRequest(TMessageContext context);
    }
}
