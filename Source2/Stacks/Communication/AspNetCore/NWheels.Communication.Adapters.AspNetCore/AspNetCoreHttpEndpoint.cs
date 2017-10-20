using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NWheels.Communication.Api;
using NWheels.Communication.Api.Http;
using NWheels.Microservices.Api;

namespace NWheels.Communication.Adapters.AspNetCore
{
    public class AspNetCoreHttpEndpoint : LifecycleComponentBase, IHttpEndpoint
    {
        public void AddMiddleware(ICommunicationMiddleware<HttpContext> middleware)
        {
            throw new NotImplementedException();
        }

        public string Name => throw new NotImplementedException();

        public string Protocol => throw new NotImplementedException();

        public IEnumerable<string> ListenUrls => throw new NotImplementedException();

        public IEnumerable<ICommunicationMiddleware> Pipeline => throw new NotImplementedException();
    }
}
