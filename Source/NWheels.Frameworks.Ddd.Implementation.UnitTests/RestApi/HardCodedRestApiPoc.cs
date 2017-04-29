using NWheels.Platform.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NWheels.Frameworks.Ddd.Implementation.UnitTests.RestApi
{
    public class HardCodedRestApiPoc
    {

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TransactionScriptComponent]
        private class IcecreamConcentrationJournalTx
        {
            [TransactionScriptMethod]
            public SensorValueStatus AddSensorReading(string sensorId, DateTime utcDate, decimal value)
            {
                return SensorValueStatus.Normal;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [TransactionScriptMethod]
            public void RemoveSensorReading(string sensorId, DateTime utcDate)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private enum SensorValueStatus
        {
            Good,
            Normal,
            Bad
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ResourceHandler_Of_IcecreamConcentrationJournalTx_AddSensorReading : IRestResourceHandler
        {

            public string UriPath => "ddd/tx/HardCodedRestApiPoc.IcecreamConcentrationJournalTx/AddSensorReading";
            public IReadOnlyList<HttpMethod> SupportedHttpMethods => new[] { HttpMethod.Post };

            public Task HttpDelete(HttpContext context)
            {
                throw new NotImplementedException();
            }

            public Task HttpGet(HttpContext context)
            {
                throw new NotImplementedException();
            }

            public Task HttpPatch(HttpContext context)
            {
                throw new NotImplementedException();
            }

            public Task HttpPost(HttpContext context)
            {
                throw new NotImplementedException();
            }

            public Task HttpPut(HttpContext context)
            {
                throw new NotImplementedException();
            }
        }

    }
}
