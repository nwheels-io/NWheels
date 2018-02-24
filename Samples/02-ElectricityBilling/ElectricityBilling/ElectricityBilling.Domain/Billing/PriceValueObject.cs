using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElectricityBilling.Domain.Basics;

namespace ElectricityBilling.Domain.Billing
{
    public struct PriceValueObject
    {
        private readonly MoneyValueObject _value;
        private readonly IReadOnlyList<string> _derivationMemos;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PriceValueObject(MoneyValueObject value, IEnumerable<string> derivationMemos)
        {
            _value = value;
            _derivationMemos = derivationMemos.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MoneyValueObject Value => _value;
        public IReadOnlyList<string> DerivationMemos => _derivationMemos;
    }
}
