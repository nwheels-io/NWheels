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
    class Program
    {
        static int Main(string[] args)
        {
            return Microservice.RunCli("SensorService", args, host => {
                host.UseLogging<ElasticStack>();
                host.UseDB<EFCoreStack>();
                host.UseDdd();
                host.UseRestApi<AspNetCoreSwaggerStack>();
                host.UseUidl<WebReactReduxStack>();
                host.UseApplicationFeature<AutoDiscoverAssemblyOf<ElectricityBillingContext>>();
                host.UseMicroserviceXml("sensor-hub-integration.xml");
                host.UseLifecycleComponent<SensorReadingSubscription>();
                host.UseRestApiResources(catalog => {
                    catalog.AddDomainContextTx<ElectricityBillingContext>(x => x.StoreSensorReadingTx(null, DateTime.MinValue, 0));
                });
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        class SensorReadingSubscription : LifecycleComponentBase
        {
            private readonly ISensorHubService _sensorHub;
            private readonly ITransactionFactory _transactionFactory;
            private readonly Func<ElectricityBillingContext> _domainContextFactory;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SensorReadingSubscription(
                ISensorHubService sensorHub, 
                ITransactionFactory transactionFactory, 
                Func<ElectricityBillingContext> domainContextFactory)
            {
                _sensorHub = sensorHub;
                _transactionFactory = transactionFactory;
                _domainContextFactory = domainContextFactory;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Activate()
            {
                _sensorHub.SensorReadingReceived += HandleSensorReading;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task HandleSensorReading(DateTime timestamp, string sensorId, decimal kwh)
            {
                using (var unitOfWork = _transactionFactory.NewUnitOfWork())
                {
                    var domainContext = _domainContextFactory();
                    await domainContext.StoreSensorReadingTx(sensorId, timestamp, kwh);
                    await unitOfWork.Commit();
                }
            }
        }
    }
}
