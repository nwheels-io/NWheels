using System;
using ElectricityBilling.Domain;
using ElectricityBilling.Domain.Payments;
using NWheels.DB;
using NWheels.Ddd;
using NWheels.Logging;
using NWheels.Microservices;
using NWheels.RestApi;

namespace ElectricityBilling.PaymentService
{
    class Program
    {
        static int Main(string[] args)
        {
            return Microservice.RunDaemonCli("PaymentService", args, host => {
                host.UseLogging<ElasticStack>();
                host.UseDB<EFCoreStack>();
                host.UseDdd();
                host.UseRestApi<AspNetCoreSwaggerStack>();
                host.UseApplicationFeature<AutoDiscoverAssemblyOf<ElectricityBillingContext>>();
                host.UseMicroserviceXml("payment-processor-integrations.xml");
                host.ExposeRestApiResources(catalog => {
                    catalog.AddDomainRepository<ElectricityBillingContext, PaymentMethodEntity>();
                    catalog.AddDomainRepository<ElectricityBillingContext, ReceiptEntity>();
                });
            });
        }
    }
}
