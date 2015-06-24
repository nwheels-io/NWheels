using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace NWheels.Stacks.OdataOwinWebapi
{
    public class PingController : ApiController
    {
        [HttpGet]
        [Route("ping")]
        public string Get()
        {
            return "I'm here";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("echo/{text}")]
        public string Get(string text)
        {
            return "Echoing: " + text;
        }
    }
}
