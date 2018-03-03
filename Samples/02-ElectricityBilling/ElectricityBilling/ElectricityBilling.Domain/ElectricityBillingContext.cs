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

        IAsyncQuery<SensorContractView> 
            QuerySensorsWithValidContractsWithinPeriod(
                ICollection<CustomerEntity.Ref> optionalCustomers, BillingPeriodValueObject billingPeriod);

        IAsyncQuery<IAsyncGrouping<SensorEntity.Ref, SensorReadingValueObject>> 
            QueryReadingsWithinPeriodGroupedBySensor(
                ICollection<SensorEntity.Ref> sensors, BillingPeriodValueObject billingPeriod);

        //IAsyncQuery<ContractEntity> 
        //    QueryValidContractsForSensor(
        //        SensorEntity.Ref sensor, BillingPeriodValueObject billingPeriod);

        //IAsyncQuery<KeyValuePair<SensorEntity.Ref, ContractEntity.Ref>>
        //    QuerySensorToContractMap(
        //        ICollection<SensorEntity.Ref> sensorRefs);

        //IAsyncQuery<KeyValuePair<ContractEntity.Ref, CustomerEntity.Ref>>
        //    QueryContractToCustomerMap(
        //        ICollection<ContractEntity.Ref> contractRefs);

        //IAsyncQuery<KeyValuePair<ContractEntity.Ref, PricingPlanEntity.Ref>>
        //    QueryContractToPricingPlanMap(
        //        ICollection<ContractEntity.Ref> contractRefs);

        IAsyncQuery<KeyValuePair<CustomerEntity.Ref, CustomerEntity>>
            QueryCustomersCache(
                ICollection<CustomerEntity.Ref> customerRefs);

        IAsyncQuery<KeyValuePair<PricingPlanEntity.Ref, PricingPlanEntity>>
            QueryPricingPlansCache(
                ICollection<PricingPlanEntity.Ref> pricingPlanRefs);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void InvalidateAllInvoicesWithinPeriod(
            BillingPeriodValueObject billingPeriod);

        InvoiceEntity 
            NewInvoice(
                CustomerEntity.Ref customer, BillingPeriodValueObject billingPeriod, IEnumerable<InvoiceRowValueObject> rows);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Task StoreReadingTx(
            string sensorId, DateTime utc, decimal kwh);

        Task AssignContractTx(
            CustomerEntity.Ref customer, SensorEntity.Ref sensor, PricingPlanEntity.Ref pricingPlan);

        Task<PricingPlanEntity> GetPricingPlanAsync(
            PricingPlanEntity.Ref pricingPlanRef);

        Task<CustomerLoginResult> 
            CustomerLoginTx(
                string email, string password, bool remember);

        Task<CustomerLoginResult>
            CustomerLoginByCookieTx(
                string loginCookie);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [NWheels.TypeContract.DataTransferObject]
    public class CustomerLoginResult
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string LoginCookie { get; set; }
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
        private readonly IView<SensorContractView> _sensorContracts;
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

        public IAsyncQuery<SensorContractView> 
            QuerySensorsWithValidContractsWithinPeriod(
                ICollection<CustomerEntity.Ref> optionalCustomerRefs, BillingPeriodValueObject billingPeriod)
        {
            return _sensorContracts.Query(view => {
                var query = view;

                if (optionalCustomerRefs != null)
                {
                    query = query.Where(x => optionalCustomerRefs.Contains(x.Contract.Customer.Id));
                }

                query = query.Where(x =>
                    x.Contract.ValidFrom <= billingPeriod.EndDate &&
                    (x.Contract.ValidUntil == null || x.Contract.ValidUntil >= billingPeriod.StartDate));

                return query;
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<IAsyncGrouping<SensorEntity.Ref, SensorReadingValueObject>> 
            QueryReadingsWithinPeriodGroupedBySensor(
                ICollection<SensorEntity.Ref> sensorRefs, BillingPeriodValueObject billingPeriod)
        {
            return _sensorReadings
                .Query(q => q
                    .Where(x => 
                        sensorRefs.Contains(x.Sensor.Id) && 
                        x.UtcTimestamp >= billingPeriod.StartDate && 
                        x.UtcTimestamp <= billingPeriod.EndDate)
                    .OrderBy(x => x.UtcTimestamp))
                .GroupBy(x => x.Sensor);
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public IAsyncQuery<ContractEntity> QueryValidContractsForSensor(SensorEntity.Ref sensor, BillingPeriodValueObject billingPeriod)
        //{
        //    var query =_contracts
        //        .Query(q => q
        //            .Where(x => 
        //                x.Sensor.Id == sensor.Id && 
        //                x.ValidFrom <= billingPeriod.EndDate && 
        //                (x.ValidUntil == null || x.ValidUntil >= billingPeriod.StartDate))
        //            .OrderBy(x => x.ValidFrom));

        //    return query;
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public IAsyncQuery<KeyValuePair<SensorEntity.Ref, ContractEntity.Ref>> 
        //    QuerySensorToContractMap(
        //        ICollection<SensorEntity.Ref> sensorRefs)
        //{
        //    return _contracts.Query(allContracts =>
        //        from contract in allContracts
        //        join sensor in _sensors on contract.Sensor.Id equals sensor.Id
        //        where sensorRefs.Contains(sensor.Id)
        //        select new KeyValuePair<SensorEntity.Ref, ContractEntity.Ref>(sensor.Id, contract.Id));
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public IAsyncQuery<KeyValuePair<ContractEntity.Ref, CustomerEntity.Ref>>
        //    QueryContractToCustomerMap(
        //        ICollection<ContractEntity.Ref> contractRefs)
        //{
        //    return _contracts.Query(q => q
        //        .Where(x => contractRefs.Contains(x.Id))
        //        .Select(x => new KeyValuePair<ContractEntity.Ref, CustomerEntity.Ref>(x, x.Customer)));
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public IAsyncQuery<KeyValuePair<ContractEntity.Ref, PricingPlanEntity.Ref>>
        //    QueryContractToPricingPlanMap(
        //        ICollection<ContractEntity.Ref> contractRefs)
        //{
        //    return _contracts.Query(q => q
        //        .Where(x => contractRefs.Contains(x.Id))
        //        .Select(x => new KeyValuePair<ContractEntity.Ref, PricingPlanEntity.Ref>(x, x.PricingPlan)));
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<KeyValuePair<CustomerEntity.Ref, CustomerEntity>> 
            QueryCustomersCache(
                ICollection<CustomerEntity.Ref> customerRefs)
        {
            return _customers.Query(q => q
                .Where(cust => customerRefs.Contains(cust.Id))
                .Select(cust => new KeyValuePair<CustomerEntity.Ref, CustomerEntity>(cust.Id, cust)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAsyncQuery<KeyValuePair<PricingPlanEntity.Ref, PricingPlanEntity>> 
            QueryPricingPlansCache(
                ICollection<PricingPlanEntity.Ref> pricingPlanRefs)
        {
            return _pricingPlans.Query(q => q
                .Where(plan => pricingPlanRefs.Contains(plan.Id))
                .Select(plan => new KeyValuePair<PricingPlanEntity.Ref, PricingPlanEntity>(plan.Id, plan)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InvoiceEntity NewInvoice(CustomerEntity.Ref customer, BillingPeriodValueObject billingPeriod, IEnumerable<InvoiceRowValueObject> rows)
        {
            var injector = _injectorFactory.Create<CustomerEntity, IOperatingSystemEnvironment>();
            return _invoices.New(() => new InvoiceEntity(injector, customer, billingPeriod, rows));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task StoreReadingTx(string sensorId, DateTime utc, decimal kwh)
        {
            using (var unitOfWork = _transactionFactory.NewUnitOfWork())
            {
                _sensorReadings.New(constructor: () => new SensorReadingValueObject(sensorId, utc, kwh));
                await unitOfWork.CommitAsync();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task<CustomerLoginResult> CustomerLoginTx(string email, string password, bool remember)
        {
            //TBD
            return new CustomerLoginResult();
        }

        public Task<CustomerLoginResult> CustomerLoginByCookieTx(string loginCookie)
        {
            throw new NotImplementedException();
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

                await unitOfWork.CommitAsync();
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
