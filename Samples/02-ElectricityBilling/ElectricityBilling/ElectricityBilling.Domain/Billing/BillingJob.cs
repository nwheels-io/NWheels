using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IElectricityBillingContext _context;
        private readonly ITransactionFactory _transactionFactory;
        private readonly Arguments _arguments;
        private readonly BillingPeriodValueObject _billingPeriod;
        private readonly Dictionary<ContractEntity.Ref, PriceValueObject> _pricePerContract;
        private readonly Dictionary<ContractEntity.Ref, CustomerEntity.Ref> _contractCustomerMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BillingJob(
            IOperatingSystemEnvironment environment, 
            IElectricityBillingContext context, 
            ITransactionFactory transactionFactory, 
            Arguments arguments)
        {
            _environment = environment;
            _context = context;
            _transactionFactory = transactionFactory;
            _arguments = arguments;
            _billingPeriod = BillingPeriodValueObject.CreateMonthly(arguments.Year, arguments.Month);
            _pricePerContract = new Dictionary<ContractEntity.Ref, PriceValueObject>();
            _contractCustomerMap = new Dictionary<ContractEntity.Ref, CustomerEntity.Ref>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task Execute()
        {
            using (var unitOfWork = _transactionFactory.NewUnitOfWork())
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

                var assignedSensors = await _context.QueryAssignedSensorsWithinPeriod(specificCustomers, _billingPeriod).ToListAsync();

                var readingsGroupedBySensorQuery = _context.QueryReadingsWithinPeriodGroupedBySensor(assignedSensors, _billingPeriod);
                await readingsGroupedBySensorQuery.ForEachAsync(group => ProcessSensorReadingsAsync(sensor: group.Key, readings: group));

                await unitOfWork.Commit();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task ProcessSensorReadingsAsync(SensorEntity.Ref sensor, IAsyncEnumerable<SensorReadingValueObject> readings)
        {
            using (var readingEnumerator = await readings.GetEnumeratorAsync())
            {
                var contracts = _context.QueryValidContractsForSensor(sensor, _billingPeriod);

                if (await readingEnumerator.MoveNextAsync())
                {
                    await contracts.ForEachAsync(async contract => {
                        // ReSharper disable once AccessToDisposedClosure
                        var price = await CalculatePriceForContract(contract, readingEnumerator);
                        _pricePerContract[contract] = price;
                    });
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task<PriceValueObject> CalculatePriceForContract(ContractEntity contract, IAsyncEnumerator<SensorReadingValueObject> readingEnumerator)
        {
            int skippedBeforeContract = 0;
            var contractReadings = new List<SensorReadingValueObject>(capacity: GetEstimatedNumberOfReadings(contract));

            do
            {
                var reading = readingEnumerator.Current;

                if (reading.UtcTimestamp < contract.ValidFrom)
                {
                    skippedBeforeContract++;
                }
                else if (reading.UtcTimestamp >= contract.ValidUntil)
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
                //TODO: log warning
            }

            var pricingPlan = await _context.GetPricingPlanAsync(contract.PricingPlan);
            var price = await pricingPlan.CalculatePriceAsync(contractReadings.AsAsync());

            return price;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task CreateInvoices()
        {
            //TODO: implement this

            var contracts = _pricePerContract.Keys.ToList();
            await _context.QueryContractCustomerMap(contracts).ForEachAsync(pair => {
                _contractCustomerMap[pair.Contract] = pair.Customer;
                return Task.CompletedTask;
            });

            var contractGroupsByCustomer = _contractCustomerMap.GroupBy(x => x.Value).ToList();

            //TBD...
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
    }
}
