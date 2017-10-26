using System.Threading.Tasks;

namespace NWheels.RestApi.Api
{
    public interface IResourceBinding
    {
        string Protocol { get; }
    }

    public interface IResourceBinding<in TMessageContext> : IResourceBinding
    {
        Task HandleRequest(TMessageContext context);
    }
}
