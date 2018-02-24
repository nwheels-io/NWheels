using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricityBilling.Domain.Billing;
using ElectricityBilling.Domain.Customers;
using ElectricityBilling.Domain.Payments;
using ElectricityBilling.Domain.Sensors;
using NWheels;
using NWheels.DB;
using NWheels.Ddd;
using NWheels.Transactions;
using Ddd = NWheels.Ddd;

namespace ElectricityBilling.Domain
{
    public interface IElectricityBillingContext
    {
        IAsyncQuery<CustomerEntity.Ref>
            QueryMissingInvoiceCustomers(
                BillingPeriodValueObject billingPeriod);

        IAsyncQuery<PricingPlanContractView> 
            QueryCustomerContractsByPricingPlan(
                PricingPlanEntity.Ref pricingPlan);

        IAsyncQuery<SensorEntity.Ref> 
            QueryAssignedSensorsWithinPeriod(
                IList<CustomerEntity.Ref> optionalCustomers, BillingPeriodValueObject billingPeriod);

        IAsyncQuery<IAsyncGrouping<SensorEntity.Ref, SensorReadingValueObject>> 
            QueryReadingsWithinPeriodGroupedBySensor(
                IList<SensorEntity.Ref> sensors, BillingPeriodValueObject billingPeriod);

        IAsyncQuery<ContractEntity> 
            QueryValidContractsForSensor(
                SensorEntity.Ref sensor, BillingPeriodValueObject billingPeriod);

        IAsyncQuery<ContractCustomerMap>
            QueryContractCustomerMap(
                IList<ContractEntity.Ref> contracts);

        Task StoreReadingTx(
            string sensorId, DateTime utc, decimal kwh);

        Task AssignContractTx(
            CustomerEntity.Ref customer, SensorEntity.Ref sensor, PricingPlanEntity.Ref pricingPlan);

        void InvalidateAllInvoicesWithinPeriod(
            BillingPeriodValueObject billingPeriod);

        Task<PricingPlanEntity> GetPricingPlanAsync(
            PricingPlanEntity.Ref pricingPlanRef);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct ContractCustomerMap
    {
        public ContractCustomerMap(ContractEntity.Ref contract, CustomerEntity.Ref customer)
        {
            Contract = contract;
            Customer = customer;
        }

        public readonly ContractEntity.Ref Contract;
        public readonly CustomerEntity.Ref Customer;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [Ddd.TypeContract.BoundedContext]
    public class ElectricityBillingContext : IElectricityBillingContext
    {
        private readonly ITransactionFactory _transactionFactory;
        private readonly IInjectorFactory _injectorFactory;
        private readonly IRepository<CustomerEntity> _customers;
        private readonly IRepository<SensorEntity> _sensors;
        private readonly IRepository<ContractEntity> _contracts;
        private readonly IRepository<PricingPlanEntity> _pricingPlans;
        private readonly IRepository<SensorReadingValueObject> _sensorReadings;
        private readonly IRepository<InvoiceEntity> _invoices;
        private readonly IRepository<ReceiptEntity> _receipts;
        private readonly IRepository<PaymentMethodEntity> _paymentMethods;
        private readonly IView<PricingPlanContractView> _pricingPlanContracts;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ElectricityBillingContext(ITransactionFactory transactionFactory, IInjectorFactory injectorFactory)
        {
            _transactionFactory = transactionFactory;
            _injectorFactory = injectorFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<CustomerEntity.Ref> QueryMissingInvoiceCustomers(BillingPeriodValueObject billingPeriod)
        {
            return _customers.Query(allCustomers =>
                from customer in allCustomers
                from invoice in _invoices.Where(invoice =>
                    invoice.Customer.Id == customer.Id &&
                    invoice.InvalidatedAt == null && 
                    invoice.BillingPeriod.StartDate >= billingPeriod.StartDate &&
                    invoice.BillingPeriod.StartDate < billingPeriod.EndDate
                ).DefaultIfEmpty()
                where invoice == null
                select new CustomerEntity.Ref(customer.Id));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<PricingPlanContractView> QueryCustomerContractsByPricingPlan(PricingPlanEntity.Ref pricingPlan)
        {
            return _pricingPlanContracts.Query(q => q.Where(x => x.PricingPlan.Id == pricingPlan.Id));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<SensorEntity.Ref> QueryAssignedSensorsWithinPeriod(
            IList<CustomerEntity.Ref> optionalCustomers, BillingPeriodValueObject billingPeriod)
        {
            var sensorRefQuery = _contracts.Query(q => 
                getContractQuery(q)
                .Select(x => x.Sensor)
                .Distinct());

            return sensorRefQuery;
            
            IQueryable<ContractEntity> getContractQuery(IQueryable<ContractEntity> source)
            {
                var contractQuery = source;

                if (optionalCustomers != null)
                {
                    contractQuery = contractQuery.Where(x => optionalCustomers.Contains(x.Customer));
                }

                contractQuery = contractQuery.Where(x =>
                    x.ValidFrom <= billingPeriod.EndDate &&
                    (x.ValidUntil == null || x.ValidUntil >= billingPeriod.StartDate));

                return contractQuery;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<IAsyncGrouping<SensorEntity.Ref, SensorReadingValueObject>> 
            QueryReadingsWithinPeriodGroupedBySensor(
                IList<SensorEntity.Ref> sensors, BillingPeriodValueObject billingPeriod)
        {
            return _sensorReadings
                .Query(q => q
                    .Where(x => 
                        sensors.Contains(x.Sensor) && 
                        x.UtcTimestamp >= billingPeriod.StartDate && 
                        x.UtcTimestamp <= billingPeriod.EndDate)
                    .OrderBy(x => x.UtcTimestamp))
                .GroupBy(x => x.Sensor);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<ContractEntity> QueryValidContractsForSensor(SensorEntity.Ref sensor, BillingPeriodValueObject billingPeriod)
        {
            var query =_contracts
                .Query(q => q
                    .Where(x => 
                        x.Sensor.Id == sensor.Id && 
                        x.ValidFrom <= billingPeriod.EndDate && 
                        (x.ValidUntil == null || x.ValidUntil >= billingPeriod.StartDate))
                    .OrderBy(x => x.ValidFrom));

            return query;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<ContractCustomerMap> QueryContractCustomerMap(IList<ContractEntity.Ref> contracts)
        {
            return _contracts.Query(q => q.Where(x => contracts.Contains(x.Id)).Select(x => new ContractCustomerMap(x, x.Customer)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task StoreReadingTx(string sensorId, DateTime utc, decimal kwh)
        {
            using (var unitOfWork = _transactionFactory.NewUnitOfWork())
            {
                _sensorReadings.New(constructor: () => new SensorReadingValueObject(sensorId, utc, kwh));
                await unitOfWork.Commit();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task AssignContractTx(
            CustomerEntity.Ref customer,
            SensorEntity.Ref sensor,
            PricingPlanEntity.Ref pricingPlan)
        {
            using (var unitOfWork = _transactionFactory.NewUnitOfWork())
            {
                var newContract = _contracts.New(() => new ContractEntity(
                    _injectorFactory.Create<ContractEntity, ContractEntity.IMyLocalizables, IOperatingSystemEnvironment>(),
                    customer, sensor, pricingPlan));

                var contractToReplace = await _contracts
                    .Query(q => q.Where(c => c.Customer.Id == customer.Id && c.PricingPlan.Id == pricingPlan.Id))
                    .FirstOrDefaultAsync();

                if (contractToReplace != null)
                {
                    contractToReplace.ReplaceWith(newContract);
                }

                await unitOfWork.Commit();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InvalidateAllInvoicesWithinPeriod(BillingPeriodValueObject billingPeriod)
        {
            InvoiceEntity.BulkInvalidateBillingPeriod(_invoices, billingPeriod);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task<PricingPlanEntity> GetPricingPlanAsync(PricingPlanEntity.Ref pricingPlanRef)
        {
            return _pricingPlans.Query(q => q.Where(x => x.Id == pricingPlanRef.Id)).FirstAsync();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private IRepository<CustomerEntity> Customers => _customers;
        //private IRepository<SensorEntity> Sensors => _sensors;
        //private IRepository<ContractEntity> Contracts => _contracts;
        //private IRepository<SensorReadingValueObject> SensorReadings => _sensorReadings;
        //private IView<PricingPlanContractView> PricingPlanContracts => _pricingPlanContracts;
    }
}
