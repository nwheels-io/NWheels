using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectricityBilling.Domain.Basics;
using ElectricityBilling.Domain.Sensors;
using NWheels;
using NWheels.DB;
using NWheels.Ddd;
using MemberContract = NWheels.MemberContract;

namespace ElectricityBilling.Domain.Billing
{
    [NWheels.TypeContract.Presentation.DefaultFormat("{PricePerKwh:MoneyValueObject:C}/kWh")]
    public class FlatRatePricingPlanEntity : PricingPlanEntity
    {
        private readonly IFlatRateLocalizables _localizables;
        private MoneyValueObject _pricePerKwh;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FlatRatePricingPlanEntity(Injector<IFlatRateLocalizables> injector)
            : base(injector.Factory.Create<PricingPlanEntity, IBaseLocalizables>())
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FlatRatePricingPlanEntity(Injector<IFlatRateLocalizables> injector, string description, MoneyValueObject pricePerKwh, bool isReadOnly = false) 
            : base(injector.Factory.Create<PricingPlanEntity, IBaseLocalizables>(), description, isReadOnly)
        {
            injector.Inject(out _localizables);
            _pricePerKwh = pricePerKwh;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override async Task<PriceValueObject> CalculatePriceAsync(IAsyncEnumerable<SensorReadingValueObject> orderedReadings)
        {
            using (var enumerator = await orderedReadings.GetEnumeratorAsync())
            {
                if (await enumerator.MoveNextAsync())
                {
                    var firstReading = enumerator.Current;
                    var lastReading = firstReading;

                    while (await enumerator.MoveNextAsync())
                    {
                        lastReading = enumerator.Current;
                    }

                    var totalKwh = lastReading.KwhValue - firstReading.KwhValue;
                    if (totalKwh < 0)
                    {
                        throw new BrokenInvariantDomainException("A subsequent reading cannot be higher than a preceeding one.");
                    }

                    var calculatedPrice = totalKwh * _pricePerKwh;
                    return new PriceValueObject(calculatedPrice, derivationMemos: new string[] {
                        _localizables.FirstReading(ref firstReading),
                        _localizables.LastReading(ref lastReading),
                        _localizables.PeriodConsumptionKwh(totalKwh),
                        _localizables.FlatRate(ref _pricePerKwh),
                        _localizables.TotalPrice(ref calculatedPrice)
                    });
                }
            }

            return new PriceValueObject(0 * _pricePerKwh, new string[0]);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MemberContract.Validation.NonNegative]
        public MoneyValueObject PricePerKwh => _pricePerKwh;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [NWheels.I18n.TypeContract.Localizables(DefaultCulture = "en-US")]
        public interface IFlatRateLocalizables
        {
            [NWheels.I18n.MemberContract.InDefaultCulture("Begin of period: {reading.KwhValue:0.0000} kWh, read on {reading.UtcTimestamp:D}")]
            string FirstReading(ref SensorReadingValueObject reading);

            [NWheels.I18n.MemberContract.InDefaultCulture("End of period: {reading.KwhValue:0.0000} kWh, read on {reading.UtcTimestamp:D}")]
            string LastReading(ref SensorReadingValueObject reading);

            [NWheels.I18n.MemberContract.InDefaultCulture("Consumption during the period: {consumptionKwh:0.0000} kWh")]
            string PeriodConsumptionKwh(decimal consumptionKwh);

            [NWheels.I18n.MemberContract.InDefaultCulture("Flat rate: {pricePerKwh:MoneyValueObject:C} per kWh")]
            string FlatRate(ref MoneyValueObject pricePerKwh);

            [NWheels.I18n.MemberContract.InDefaultCulture("Total price: {price:MoneyValueObject:C}")]
            string TotalPrice(ref MoneyValueObject price);
        }
    }
}
                