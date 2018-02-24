using System;
using System.Collections.Generic;
using System.Text;
using NWheels;

namespace ElectricityBilling.Domain.Basics
{
    [TypeContract.Presentation.DefaultFormat("{Amount:#,##0.00} {CurrencyCode}")]
    public struct MoneyValueObject
    {
        private readonly decimal _amount;
        private readonly string _currencyCode;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MoneyValueObject(decimal amount, string currencyCode = null)
        {
            _amount = amount;
            _currencyCode = currencyCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public decimal Amount => _amount;
        public string CurrencyCode => _currencyCode;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MoneyValueObject operator *(decimal x, MoneyValueObject y)
        {
            return new MoneyValueObject(x * y._amount, y._currencyCode);
        }
    }
}
