using System;
using ElectricityBilling.Domain;
using ElectricityBilling.Domain.Billing;
using ElectricityBilling.Domain.Customers;
using NWheels.DB;
using NWheels.Ddd;
using NWheels.Logging;
using NWheels.Microservices;
using NWheels.RestApi;

namespace ElectricityBilling.CustomerService
{
    public static class Program
    {
        static int Main(string[] args)
        {
            return Microservice.RunDaemonCli("CustomerService", args, host => {
                host.UseLogging<ElasticStack>();
                host.UseDB<EFCoreStack>();
                host.UseDdd();
                host.UseRestApi<AspNetCoreSwaggerStack>();
                host.UseApplicationFeature<AutoDiscoverAssemblyOf<ElectricityBillingContext>>();
                host.ExposeRestApiResources(catalog => {
                    catalog.AddDomainRepository<ElectricityBillingContext, CustomerEntity>();
                });
            });
        }
    }
}
