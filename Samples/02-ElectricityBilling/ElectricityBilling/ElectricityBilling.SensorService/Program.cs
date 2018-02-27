using System;
using System.Threading.Tasks;
using NWheels.Microservices;
using ElectricityBilling.Domain;
using ElectricityBilling.Domain.Sensors;
using NWheels.DB;
using NWheels.Logging;
using NWheels.RestApi;
using NWheels.Transactions;
using NWheels.UI;
using NWheels.Ddd;

namespace ElectricityBilling.SensorService
{
    public static class Program
    {
        static int Main(string[] args)
        {
            return Microservice.RunDaemonCli("SensorService", args, host => {
                host.UseLogging<ElasticStack>();
                host.UseDB<EFCoreStack>();
                host.UseDdd();
                host.UseRestApi<AspNetCoreSwaggerStack>();
                //host.UseUidl<WebReactReduxStack>();
                host.UseApplicationFeature<AutoDiscoverAssemblyOf<ElectricityBillingContext>>();
                host.UseMicroserviceXml("sensor-hub-integration.xml");
                host.UseLifecycleComponent<SensorReadingSubscription>();
                host.ExposeRestApiResources(catalog => {
                    catalog.AddDomainTransaction<ElectricityBillingContext>(x => x.StoreReadingTx(null, DateTime.MinValue, 0));
                    catalog.AddDomainRepository<ElectricityBillingContext, SensorEntity>();
                });
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SensorReadingSubscription : LifecycleComponentBase
        {
            private readonly ISensorHubService _sensorHub;
            private readonly IElectricityBillingContext _domainContext;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SensorReadingSubscription(ISensorHubService sensorHub, IElectricityBillingContext domainContext)
            {
                _sensorHub = sensorHub;
                _domainContext = domainContext;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Activate()
            {
                _sensorHub.SensorReadingReceived += HandleSensorReading;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task HandleSensorReading(DateTime timestamp, string sensorId, decimal kwh)
            {
                await _domainContext.StoreReadingTx(sensorId, timestamp, kwh);
            }
        }
    }
}
