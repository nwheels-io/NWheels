using System;
using System.Collections.Generic;
using System.Text;
using NWheels;

namespace ElectricityBilling.Domain.Basics
{
    [TypeContract.Presentation.DefaultFormat("{Amount:#,##0.00} {CurrencyCode}")]
    public struct MoneyValueObject
    {
        public MoneyValueObject(decimal amount, string currencyCode = null)
        {
            this.Amount = amount;
            this.CurrencyCode = currencyCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public decimal Amount { get; }
        public string CurrencyCode { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MoneyValueObject operator *(decimal x, MoneyValueObject y)
        {
            return new MoneyValueObject(x * y.Amount, y.CurrencyCode);
        }
    }
}
