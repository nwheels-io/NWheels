using System;
using ElectricityBilling.Domain;
using ElectricityBilling.Domain.Billing;
using ElectricityBilling.Domain.Customers;
using NWheels.DB;
using NWheels.Ddd;
using NWheels.Logging;
using NWheels.Microservices;
using NWheels.RestApi;

namespace ElectricityBilling.BillingService
{
    class Program
    {
        static int Main(string[] args)
        {
            return Microservice.RunDaemonCli("BillingService", args, host => {
                host.UseLogging<ElasticStack>();
                host.UseDB<EFCoreStack>();
                host.UseDdd();
                host.UseRestApi<AspNetCoreSwaggerStack>();
                host.UseApplicationFeature<AutoDiscoverAssemblyOf<ElectricityBillingContext>>();
                host.ExposeRestApiResources(catalog => {
                    catalog.AddDomainRepository<ElectricityBillingContext, ContractEntity>();
                    catalog.AddDomainRepository<ElectricityBillingContext, PricingPlanEntity>();
                    catalog.AddDomainRepository<ElectricityBillingContext, InvoiceEntity>();
                    catalog.AddDomainTransaction<ElectricityBillingContext>(x => x.AssignContractTx(0, "", 0));
                });
            });
        }
    }
}
