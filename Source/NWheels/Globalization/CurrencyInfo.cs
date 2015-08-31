using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Globalization
{
    public class CurrencyInfo : IEquatable<CurrencyInfo>
    {
        public CurrencyInfo(int isoNumericCode, IEnumerable<RegionInfo> regions)
        {
            this.IsoNumericCode = isoNumericCode;
            this.Regions = new ReadOnlyCollection<RegionInfo>(regions.ToArray());

            if ( this.Regions.Count == 0 )
            {
                throw new ArgumentException("Cannot initialize CurrencyInfo from an empty regions sequence.", "regions");
            }

            var aRegion = Regions[0];

            IsoAlphabeticCode = aRegion.ISOCurrencySymbol;
            EnglishName = aRegion.CurrencyEnglishName;
            NativeName = aRegion.CurrencyNativeName;
            Symbol = aRegion.CurrencySymbol;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            return Equals(obj as CurrencyInfo);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return this.IsoNumericCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Equals(CurrencyInfo other)
        {
            return (other != null && this.IsoNumericCode == other.IsoNumericCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return IsoAlphabeticCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IFormatProvider GetFormatProvider(IFormatProvider baseFormat)
        {
            return new CurrencyFormatProvider(this, baseFormat);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IFormatProvider GetFormatProvider()
        {
            return new CurrencyFormatProvider(this, CultureInfo.CurrentCulture);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int IsoNumericCode { get; private set; }
        public string IsoAlphabeticCode { get; private set; }
        public string EnglishName { get; private set; }
        public string NativeName { get; private set; }
        public string Symbol { get; private set; }
        public IReadOnlyList<RegionInfo> Regions { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<int, CurrencyInfo> _s_byIsoNumericCode;
        private static readonly Dictionary<string, CurrencyInfo> _s_byIsoAlphabeticCode;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static CurrencyInfo()
        {
            var codeLookup = InitializeCodeLookupDictionary();

            _s_byIsoAlphabeticCode = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(culture => !culture.IsNeutralCulture)
                .Select(TryGetRegionFromCulture)
                .Where(region => region != null)
                .GroupBy(region => region.ISOCurrencySymbol)
                .Where(regionGroup => codeLookup.ContainsKey(regionGroup.Key))
                .ToDictionary(regionGroup => regionGroup.Key, regionGroup => new CurrencyInfo(
                    isoNumericCode: codeLookup[regionGroup.Key], 
                    regions: regionGroup));

            _s_byIsoNumericCode = _s_byIsoAlphabeticCode.ToDictionary(kvp => kvp.Value.IsoNumericCode, kvp => kvp.Value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator ==(CurrencyInfo x, CurrencyInfo y)
        {
            if ( !ReferenceEquals(x, null) )
            {
                return x.Equals(y);
            }
            else
            {
                return ReferenceEquals(y, null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator !=(CurrencyInfo x, CurrencyInfo y)
        {
            return !(x == y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static CurrencyInfo GetCurrency(int isoNumericCode)
        {
            return _s_byIsoNumericCode[isoNumericCode];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static CurrencyInfo GetCurrency(string isoAlphabeticCode)
        {
            return _s_byIsoAlphabeticCode[isoAlphabeticCode];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static RegionInfo TryGetRegionFromCulture(CultureInfo culture)
        {
            try
            {
                return new RegionInfo(culture.LCID);
            }
            catch
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static Dictionary<string, int> InitializeCodeLookupDictionary()
        {
            return new Dictionary<string, int>() {
                #region Codes by ISO 4217
                { "ALL",008 },
                { "DZD",012 },
                { "ARS",032 },
                { "AUD",036 },
                { "BSD",044 }, 
                { "BHD",048 },
                { "BDT",050 },
                { "AMD",051 },
                { "BBD",052 },
                { "BMD",060 },
                { "BTN",064 },
                { "BOB",068 },
                { "BWP",072 },
                { "BZD",084 },
                { "SBD",090 },
                { "BND",096 },
                { "MMK",104 },
                { "BIF",108 },
                { "KHR",116 },
                { "CAD",124 },
                { "CVE",132 },
                { "KYD",136 },
                { "LKR",144 },
                { "CLP",152 },
                { "CNY",156 },
                { "COP",170 },
                { "KMF",174 },
                { "CRC",188 },
                { "HRK",191 },
                { "CUP",192 },
                { "CZK",203 },
                { "DKK",208 },
                { "DOP",214 },
                { "SVC",222 },
                { "ETB",230 },
                { "ERN",232 },
                { "FKP",238 },
                { "FJD",242 },
                { "DJF",262 },
                { "GMD",270 },
                { "GIP",292 },
                { "GTQ",320 },
                { "GNF",324 },
                { "GYD",328 },
                { "HTG",332 },
                { "HNL",340 },
                { "HKD",344 },
                { "HUF",348 },
                { "ISK",352 },
                { "INR",356 },
                { "IDR",360 },
                { "IRR",364 },
                { "IQD",368 },
                { "ILS",376 },
                { "JMD",388 },
                { "JPY",392 },
                { "KZT",398 },
                { "JOD",400 },
                { "KES",404 },
                { "KPW",408 },
                { "KRW",410 },
                { "KWD",414 },
                { "KGS",417 },
                { "LAK",418 },
                { "LBP",422 },
                { "LSL",426 },
                { "LRD",430 },
                { "LYD",434 },
                { "MOP",446 },
                { "MWK",454 },
                { "MYR",458 },
                { "MVR",462 },
                { "MRO",478 },
                { "MUR",480 },
                { "MXN",484 },
                { "MNT",496 },
                { "MDL",498 },
                { "MAD",504 },
                { "OMR",512 },
                { "NAD",516 },
                { "NPR",524 },
                { "ANG",532 },
                { "AWG",533 },
                { "VUV",548 },
                { "NZD",554 },
                { "NIO",558 },
                { "NGN",566 },
                { "NOK",578 },
                { "PKR",586 },
                { "PAB",590 },
                { "PGK",598 },
                { "PYG",600 },
                { "PEN",604 },
                { "PHP",608 },
                { "QAR",634 },
                { "RUB",643 },
                { "RWF",646 },
                { "SHP",654 },
                { "STD",678 },
                { "SAR",682 },
                { "SCR",690 },
                { "SLL",694 },
                { "SGD",702 },
                { "VND",704 },
                { "SOS",706 },
                { "ZAR",710 },
                { "SSP",728 },
                { "SZL",748 },
                { "SEK",752 },
                { "CHF",756 },
                { "SYP",760 },
                { "THB",764 },
                { "TOP",776 },
                { "TTD",780 },
                { "AED",784 },
                { "TND",788 },
                { "UGX",800 },
                { "MKD",807 },
                { "EGP",818 },
                { "GBP",826 },
                { "TZS",834 },
                { "USD",840 },
                { "UYU",858 },
                { "UZS",860 },
                { "WST",882 },
                { "YER",886 },
                { "TWD",901 },
                { "CUC",931 },
                { "ZWL",932 },
                { "TMT",934 },
                { "GHS",936 },
                { "VEF",937 },
                { "SDG",938 },
                { "UYI",940 },
                { "RSD",941 },
                { "MZN",943 },
                { "AZN",944 },
                { "RON",946 },
                { "CHE",947 },
                { "CHW",948 },
                { "TRY",949 },
                { "XAF",950 },
                { "XCD",951 },
                { "XOF",952 },
                { "XPF",953 },
                { "XBA",955 },
                { "XBB",956 },
                { "XBC",957 },
                { "XBD",958 },
                { "XAU",959 },
                { "XDR",960 },
                { "XAG",961 },
                { "XPT",962 },
                { "XTS",963 },
                { "XPD",964 },
                { "XUA",965 },
                { "ZMW",967 },
                { "SRD",968 },
                { "MGA",969 },
                { "COU",970 },
                { "AFN",971 },
                { "TJS",972 },
                { "AOA",973 },
                { "BYR",974 },
                { "BGN",975 },
                { "CDF",976 },
                { "BAM",977 },
                { "EUR",978 },
                { "MXV",979 },
                { "UAH",980 },
                { "GEL",981 },
                { "BOV",984 },
                { "PLN",985 },
                { "BRL",986 },
                { "CLF",990 },
                { "XSU",994 },
                { "USN",997 },
                { "XXX",999 }
                #endregion
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CurrencyFormatProvider : IFormatProvider
        {
            private readonly NumberFormatInfo _formatInfo;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CurrencyFormatProvider(CurrencyInfo currencyInfo, IFormatProvider baseProvider)
            {
                var baseInfo = (NumberFormatInfo)baseProvider.GetFormat(typeof(NumberFormatInfo));

                _formatInfo = new NumberFormatInfo() {
                    CurrencySymbol = currencyInfo.Symbol,
                    CurrencyDecimalSeparator = baseInfo.CurrencyDecimalSeparator,
                    CurrencyDecimalDigits = baseInfo.CurrencyDecimalDigits,
                    CurrencyGroupSeparator = baseInfo.CurrencyGroupSeparator,
                    CurrencyGroupSizes = baseInfo.CurrencyGroupSizes,
                    CurrencyNegativePattern = baseInfo.CurrencyNegativePattern,
                    CurrencyPositivePattern = baseInfo.CurrencyPositivePattern,
                    NegativeSign = baseInfo.NegativeSign
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(NumberFormatInfo))
                {
                    return _formatInfo;
                }
                else
                {
                    throw new NotSupportedException("CurrencyInfo format provider only supports NumberFormatInfo.");
                }
            }
        }
    }
}
