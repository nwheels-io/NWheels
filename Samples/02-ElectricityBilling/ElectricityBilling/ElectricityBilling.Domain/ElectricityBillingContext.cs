using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricityBilling.Domain.Billing;
using ElectricityBilling.Domain.Customers;
using ElectricityBilling.Domain.Sensors;
using NWheels;
using NWheels.DB;
using NWheels.Ddd;
using NWheels.Transactions;
using Ddd = NWheels.Ddd;

namespace ElectricityBilling.Domain
{
    [Ddd.TypeContract.BoundedContext]
    public class ElectricityBillingContext
    {
        private readonly ITransactionFactory _transactionFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ElectricityBillingContext(ITransactionFactory transactionFactory)
        {
            _transactionFactory = transactionFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<PricingPlanContractView> QueryCustomerContractsByPricingPlan(long pricingPlanId)
        {
            return this.PricingPlanContracts.Query(q => q.Where(x => x.PricingPlanId == pricingPlanId));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task StoreSensorReadingTx(string sensorId, DateTime utc, decimal kwh)
        {
            SensorReadings.New(constructor: () => new SensorReadingValueObject(sensorId, utc, kwh));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public IView<SensorReadingValueObject> ReadOnlySensorReadings { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IRepository<CustomerEntity> Customers { get; }
        private IRepository<SensorEntity> Sensors { get; }
        private IView<PricingPlanContractView> PricingPlanContracts { get; }
        private IRepository<SensorReadingValueObject> SensorReadings { get; }
    }
}
