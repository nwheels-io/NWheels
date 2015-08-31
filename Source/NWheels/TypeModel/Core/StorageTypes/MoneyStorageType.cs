using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Extensions;
using NWheels.Globalization;

namespace NWheels.DataObjects.Core.StorageTypes
{
    public class MoneyStorageType : IStorageDataType<Money, MoneyStorageType.SerializableMoney>, IStorageContractConversionWriter
    {
        public SerializableMoney ContractToStorage(Money contractValue)
        {
            return new SerializableMoney(contractValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Money StorageToContract(SerializableMoney storageValue)
        {
            return storageValue.ToMoney();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type StorageDataType
        {
            get { return typeof(SerializableMoney); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStorageContractConversionWriter.WriteContractToStorageConversion(
            MethodWriterBase method,
            IOperand<TypeTemplate.TContract> contractValue,
            MutableOperand<TypeTemplate.TValue> storageValue)
        {
            storageValue.Assign(method.New<SerializableMoney>(contractValue).CastTo<TypeTemplate.TValue>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStorageContractConversionWriter.WriteStorageToContractConversion(
            MethodWriterBase method,
            MutableOperand<TypeTemplate.TContract> contractValue,
            IOperand<TypeTemplate.TValue> storageValue)
        {
            contractValue.Assign(storageValue.CastTo<SerializableMoney>().Func<Money>(x => x.ToMoney).CastTo<TypeTemplate.TContract>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SerializableMoney
        {
            public SerializableMoney()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SerializableMoney(Money source)
            {
                this.Amount = source.Amount;
                this.Currency = source.Currency.GetInfo().IsoAlphabeticCode;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Money ToMoney()
            {
                if ( this.Currency != null )
                {
                    var currencyInfo = CurrencyInfo.GetCurrency(this.Currency);
                    var currency = new Currency(currencyInfo.IsoNumericCode);
                    return new Money(this.Amount, currency);
                }
                else
                {
                    //TODO: handle this properly
                    return new Money(this.Amount, new Currency(CurrencyInfo.GetCurrency("USD").IsoNumericCode));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public decimal Amount { get; set; }
            public string Currency { get; set; }
        }
    }
}
