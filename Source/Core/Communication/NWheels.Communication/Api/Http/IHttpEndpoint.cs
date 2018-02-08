using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NWheels.Communication.Api.Http
{
    public interface IHttpEndpoint : ICommunicationEndpoint<HttpContext>
    {
    }
}
