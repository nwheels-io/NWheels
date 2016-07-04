using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Globalization;

namespace NWheels
{
    public struct Money : IEquatable<Money>, IComparable<Money>, IFormattable
    {
        public readonly decimal Amount;
        public readonly Currency Currency;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Money(decimal amount, Currency currency)
        {
            this.Amount = amount;
            this.Currency = currency;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Equals(Money other)
        {
            return Equals(this.Amount == other.Amount && this.Currency == other.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int CompareTo(Money other)
        {
            if (this.Currency == other.Currency)
            {
                return this.Amount.CompareTo(other.Amount);
            }
            else
            {
                throw new ArgumentException("Cannot compare Money of different currencies.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var currencyFormatProvider = this.Currency.GetInfo().GetFormatProvider(baseFormat: formatProvider);
            return this.Amount.ToString(format, currencyFormatProvider);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return ToString("C", CultureInfo.CurrentUICulture);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            if ( obj is Money )
            {
                return Equals((Money)obj);
            }
            else
            {
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return Amount.GetHashCode() ^ Currency.GetHashCode();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator decimal(Money m)
        {
            return m.Amount;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator ==(Money x, Money y)
        {
            return x.Equals(y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator !=(Money x, Money y)
        {
            return !(x == y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Money operator +(Money x, Money y)
        {
            ValidateIdenticalCurrency(x, y);
            return new Money(x.Amount + y.Amount, x.Currency.HasValue ? x.Currency : y.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Money operator -(Money x, Money y)
        {
            ValidateIdenticalCurrency(x, y);
            return new Money(x.Amount - y.Amount, x.Currency.HasValue ? x.Currency : y.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Money operator *(Money x, Money y)
        {
            ValidateIdenticalCurrency(x, y);
            return new Money(x.Amount * y.Amount, x.Currency.HasValue ? x.Currency : y.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Money operator /(Money x, Money y)
        {
            ValidateIdenticalCurrency(x, y);

            if ( y.Amount == 0 )
            {
                throw new DivideByZeroException();
            }

            return new Money(x.Amount / y.Amount, x.Currency.HasValue ? x.Currency : y.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Money operator +(Money x, decimal y)
        {
            return new Money(x.Amount + y, x.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Money operator -(Money x, decimal y)
        {
            return new Money(x.Amount - y, x.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Money operator *(Money x, decimal y)
        {
            return new Money(x.Amount * y, x.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Money operator /(Money x, decimal y)
        {
            if ( y == 0 )
            {
                throw new DivideByZeroException();
            }

            return new Money(x.Amount / y, x.Currency);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ValidateIdenticalCurrency(Money x, Money y)
        {
            if ( x.Currency.HasValue && y.Currency.HasValue && x.Currency != y.Currency )
            {
                throw new ArgumentException("Two Money instances must be of the same currency.");
            }
        }
    }
}
