using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Globalization
{
    public struct Currency : IEquatable<Currency>, IFormattable
    {
        public readonly int IsoNumericCode;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Currency(int isoNumericCode)
        {
            this.IsoNumericCode = isoNumericCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            if ( obj is Currency )
            {
                return Equals((Currency)obj);
            }
            else
            {
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return this.IsoNumericCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Equals(Currency other)
        {
            return (this.IsoNumericCode == other.IsoNumericCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return GetInfo().IsoAlphabeticCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var info = GetInfo();

            if ( string.IsNullOrEmpty(format) )
            {
                return info.IsoAlphabeticCode;
            }

            var formatChar = char.ToUpper(format[0]);

            switch ( formatChar )
            {
                case 'A':
                    return info.IsoAlphabeticCode;
                case 'B':
                    return info.NativeName;
                case 'C':
                    return info.Symbol;
                case 'D':
                    return info.IsoNumericCode.ToString(formatProvider);
                case 'E':
                    return info.EnglishName;
                default:
                    return info.IsoAlphabeticCode;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CurrencyInfo GetInfo()
        {
            return CurrencyInfo.GetCurrency(this.IsoNumericCode);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool HasValue
        {
            get
            {
                return (IsoNumericCode > 0);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator ==(Currency x, Currency y)
        {
            return x.Equals(y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator !=(Currency x, Currency y)
        {
            return !(x == y);
        }
    }
}
