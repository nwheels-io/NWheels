using NWheels.Platform.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

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
            public HttpResponseMessage Post(HttpRequestMessage request)
            {
                throw new NotImplementedException();
            }

            public HttpResponseMessage Delete(HttpRequestMessage request)
            {
                throw new NotSupportedException();
            }

            public HttpResponseMessage Get(HttpRequestMessage request)
            {
                throw new NotSupportedException();
            }

            public HttpResponseMessage Patch(HttpRequestMessage request)
            {
                throw new NotSupportedException();
            }

            public HttpResponseMessage Put(HttpRequestMessage request)
            {
                throw new NotSupportedException();
            }

            public string UriPath => "ddd/tx/HardCodedRestApiPoc.IcecreamConcentrationJournalTx/AddSensorReading";
            public IReadOnlyList<HttpMethod> SupportedHttpMethods => new[] { HttpMethod.Post };
        }

    }
}
