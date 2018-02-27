using System;
using ElectricityBilling.Domain;
using ElectricityBilling.Domain.Accounts;
using NWheels.DB;
using NWheels.Ddd;
using NWheels.Logging;
using NWheels.Microservices;
using NWheels.RestApi;

namespace ElectricityBilling.LoginService
{
    class Program
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
                    catalog.AddDomainRepository<ElectricityBillingContext, UserAccountEntity>();
                    catalog.AddDomainTransaction<ElectricityBillingContext>(x => x.CustomerLoginTx(null, null));
                });
            });
        }
    }
}
