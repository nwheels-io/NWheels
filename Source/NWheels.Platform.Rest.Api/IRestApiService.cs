using System.Net.Http;

namespace NWheels.Platform.Rest
{
    public interface IRestApiService
    {
        HttpResponseMessage HandleApiRequest(HttpRequestMessage request);
    }
}
