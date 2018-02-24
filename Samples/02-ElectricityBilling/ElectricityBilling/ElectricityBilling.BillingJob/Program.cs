using System;
using ElectricityBilling.Domain;
using NWheels.DB;
using NWheels.Ddd;
using NWheels.Logging;
using NWheels.Microservices;
using NWheels.RestApi;

namespace ElectricityBilling.BillingJob
{
    using BillingJob = ElectricityBilling.Domain.Billing.BillingJob;

    class Program
    {
        static int Main(string[] args)
        {
            return Microservice.RunBatchJobCli<BillingJob, BillingJob.Arguments>("BillingJob", args, host => {
                host.UseLogging<ElasticStack>();
                host.UseDB<EFCoreStack>();
                host.UseDdd();
                host.UseApplicationFeature<AutoDiscoverAssemblyOf<ElectricityBillingContext>>();
            });
        }
    }
}
