using NWheels.Communication.Api;

namespace NWheels.RestApi.Api
{
    public interface IRestApiProtocol : ICommunicationMiddleware
    {
        string ProtocolName { get; }
    }
}