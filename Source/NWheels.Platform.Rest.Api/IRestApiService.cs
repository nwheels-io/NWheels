using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public interface IRestApiService
    {
        Task HandleHttpRequest(HttpContext context);

        THandler GetHandler<THandler>(string uriPath) 
            where THandler : class, IRestResourceHandler;

        bool TryGetHandler<THandler>(string uriPath, out THandler handler)
            where THandler : class, IRestResourceHandler;
    }
}
