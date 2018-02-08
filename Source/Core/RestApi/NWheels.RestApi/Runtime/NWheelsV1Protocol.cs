using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NWheels.Communication.Api;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime
{
    public class NWheelsV1Protocol : ICommunicationMiddleware<HttpContext>, IRestApiProtocol
    {
        private static readonly string _s_protocolName = "nwheels.v1";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private readonly IResourceRouter _router;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public NWheelsV1Protocol(IResourceRouter router)
        {
            _router = router;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task OnMessage(HttpContext context, Func<Task> next)
        {
            if (_router.TryGetResourceBinding(context.Request.Path, _s_protocolName, out IResourceBinding<HttpContext> binding))
            {
                return binding.HandleRequest(context);
            }

            return next();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void OnError(Exception error, Action next)
        {
            next?.Invoke();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IRestApiProtocol.ProtocolName => _s_protocolName;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static string Name => _s_protocolName;
    }
}
