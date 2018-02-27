using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ElectricityBilling.Domain.Customers;
using ElectricityBilling.Domain.Sensors;
using NWheels;
using NWheels.DB;
using NWheels.Transactions;

namespace ElectricityBilling.Domain.Billing
{
    public class BillingJob
    {
        private readonly IOperatingSystemEnvironment _environment;
        private readonly ILocalizables _localizables;
        private readonly IElectricityBillingContext _context;
        private readonly ITransactionFactory _transactionFactory;
        private readonly Arguments _arguments;
        private readonly BillingPeriodValueObject _billingPeriod;
        private readonly Dictionary<SensorEntity.Ref, SensorReferenceData> _referenceDataBySensor;
        private Dictionary<CustomerEntity.Ref, CustomerEntity> _customersCache;
        private Dictionary<PricingPlanEntity.Ref, PricingPlanEntity> _pricingPlansCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BillingJob(
            IOperatingSystemEnvironment environment,
            ILocalizables localizables,
            IElectricityBillingContext context, 
            ITransactionFactory transactionFactory, 
            Arguments arguments)
        {
            _environment = environment;
            _localizables = localizables;
            _context = context;
            _transactionFactory = transactionFactory;
            _arguments = arguments;
            _billingPeriod = BillingPeriodValueObject.CreateMonthly(arguments.Year, arguments.Month);
            _referenceDataBySensor = new Dictionary<SensorEntity.Ref, SensorReferenceData>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task Execute()
        {
            using (var unitOfWork = _transactionFactory.NewUnitOfWork())
            {
                var specificCustomers = await RevertToCleanState();

                await LoadReferenceData(specificCustomers);
                await ProcessSensorReadings();

                CreateInvoices();

                await unitOfWork.CommitAsync();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task<ICollection<CustomerEntity.Ref>> RevertToCleanState()
        {
            List<CustomerEntity.Ref> specificCustomers = null;

            if (_arguments.ForceAll)
            {
                _context.InvalidateAllInvoicesWithinPeriod(_billingPeriod);
            }
            else
            {
                specificCustomers = await _context.QueryMissingInvoiceCustomers(_billingPeriod).ToListAsync();
            }

            return specificCustomers;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task LoadReferenceData(ICollection<CustomerEntity.Ref> optionalSpecificCustomers)
        {
            var referenceDataQuery = _context.QuerySensorsWithValidContractsWithinPeriod(optionalSpecificCustomers, _billingPeriod);

            await referenceDataQuery.ForEachAsync(sensorContract => {
                if (!_referenceDataBySensor.TryGetValue(sensorContract.Sensor.Id, out SensorReferenceData sensorData))
                {
                    sensorData = new SensorReferenceData(sensorContract.Sensor);
                    _referenceDataBySensor[sensorContract.Sensor.Id] = sensorData;
                }
                sensorData.AddContract(sensorContract.Contract);
            });

            await LoadCaches();

            foreach (var contractData in _referenceDataBySensor.Values.SelectMany(r => r.Contracts))
            {
                contractData.Customer = _customersCache[contractData.Contract.Customer.Id];
                contractData.PricingPlan = _pricingPlansCache[contractData.Contract.PricingPlan.Id];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task ProcessSensorReadings()
        {
            var sensorRefs = _referenceDataBySensor.Keys;
            var readingsGroupedBySensorQuery = _context.QueryReadingsWithinPeriodGroupedBySensor(sensorRefs, _billingPeriod);

            await readingsGroupedBySensorQuery.ForEachAsync(readingsGroup => {
                return ProcessOneSensorReadingsAsync(sensor: readingsGroup.Key, readings: readingsGroup);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task LoadCaches()
        {
            var customerRefs = _referenceDataBySensor.Values
                .SelectMany(refData => refData.Contracts)
                .Select(contractData => new CustomerEntity.Ref(contractData.Customer.Id))
                .ToList();

            var pricingPlanRefs = _referenceDataBySensor.Values
                .SelectMany(refData => refData.Contracts)
                .Select(contractData => new PricingPlanEntity.Ref(contractData.Contract.PricingPlan.Id))
                .ToList();

            var customersCacheTask = _context.QueryCustomersCache(customerRefs).ToDictionaryAsync();
            var pricingPlansCacheTask = _context.QueryPricingPlansCache(pricingPlanRefs).ToDictionaryAsync();
            await Task.WhenAll(customersCacheTask, pricingPlansCacheTask);

            _customersCache = customersCacheTask.Result;
            _pricingPlansCache = pricingPlansCacheTask.Result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task ProcessOneSensorReadingsAsync(SensorEntity.Ref sensor, IAsyncEnumerable<SensorReadingValueObject> readings)
        {
            using (var readingEnumerator = await readings.GetEnumeratorAsync())
            {
                var data = _referenceDataBySensor[sensor];

                foreach (var contractRefData in data.Contracts.OrderBy(x => x.Contract.ValidFrom))
                {
                    var price = await CalculatePriceForContract(contractRefData, readingEnumerator);
                    contractRefData.Price = price;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task<PriceValueObject> CalculatePriceForContract(
            ContractReferenceData contractRefData, 
            IAsyncEnumerator<SensorReadingValueObject> readingEnumerator)
        {
            int skippedBeforeContract = 0;
            var contractReadings = new List<SensorReadingValueObject>(capacity: GetEstimatedNumberOfReadings(contractRefData.Contract));

            do
            {
                var reading = readingEnumerator.Current;

                if (reading.UtcTimestamp < contractRefData.Contract.ValidFrom)
                {
                    skippedBeforeContract++;
                }
                else if (reading.UtcTimestamp >= contractRefData.Contract.ValidUntil)
                {
                    break;
                }
                else
                {
                    contractReadings.Add(reading);
                }
            } while (await readingEnumerator.MoveNextAsync());

            if (skippedBeforeContract > 0)
            {
                //TODO: log warning - readings during period not covered by any contract
            }

            var price = await contractRefData.PricingPlan.CalculatePriceAsync(contractRefData.Customer, contractReadings.AsAsync());

            return price;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateInvoices()
        {
            var contractGroupsByCustomer = _referenceDataBySensor.Values
                .SelectMany(refData => refData.Contracts)
                .GroupBy(c => new CustomerEntity.Ref(c.Customer.Id));

            foreach (var customerContractGroup in contractGroupsByCustomer)
            {
                var invoiceRows = customerContractGroup.Select(contractRefData => new InvoiceRowValueObject(
                        contractRefData.Contract.Id,
                        contractRefData.Price,
                        FormatInvoiceRowDescription(contractRefData))).ToList();

                _context.NewInvoice(customerContractGroup.Key, _billingPeriod, invoiceRows);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string FormatInvoiceRowDescription(ContractReferenceData contractRefData)
        {
            var culture = (
                contractRefData.Customer.Culture != null
                    ? CultureInfo.GetCultureInfo(contractRefData.Customer.Culture)
                    : Thread.CurrentThread.CurrentCulture);

            var description = _localizables.DescriptionPerSensorContract(
                culture, contractRefData.Sensor, contractRefData.Contract, contractRefData.PricingPlan);

            return description;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private int GetEstimatedNumberOfReadings(ContractEntity contract)
        {
            var totalSeconds = Math.Min(_billingPeriod.TimeSpan.TotalSeconds, contract.ValidityTimeSpan.TotalSeconds);
            return 2 * (int)totalSeconds;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Arguments
        {
            [NWheels.MemberContract.Semantics.Month]
            public int Month { get; set; }

            [NWheels.MemberContract.Semantics.Year]
            public int Year { get; set; }

            // if false, only missing invoices are recalculated;
            // if true, all existing invoices for the billing period are invalidated
            public bool ForceAll { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [NWheels.I18n.TypeContract.Localizables(DefaultCulture = "en-US")]
        public interface ILocalizables
        {
            [NWheels.I18n.MemberContract.InDefaultCulture("{sensor.Location:PostalAddress} (sensor # {sensor.Id}), {pricingPlan.Description}")]
            string DescriptionPerSensorContract(CultureInfo culture, SensorEntity sensor, ContractEntity contract, PricingPlanEntity pricingPlan);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SensorReferenceData
        {
            private readonly List<ContractReferenceData> _contracts = new List<ContractReferenceData>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SensorReferenceData(SensorEntity sensor)
            {
                this.Sensor = sensor;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractReferenceData AddContract(ContractEntity contract)
            {
                var contractData = new ContractReferenceData(this, contract);
                _contracts.Add(contractData);
                return contractData;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SensorEntity Sensor { get; }
            public IReadOnlyList<ContractReferenceData> Contracts => _contracts;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ContractReferenceData
        {
            public ContractReferenceData(SensorReferenceData owner, ContractEntity contract)
            {
                this.Owner = owner;
                this.Contract = contract;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SensorReferenceData Owner { get; }
            public SensorEntity Sensor => Owner.Sensor;
            public ContractEntity Contract { get; }
            public CustomerEntity Customer { get; set; }
            public PricingPlanEntity PricingPlan { get; set; }
            public PriceValueObject Price { get; set; }
        }
    }
}
