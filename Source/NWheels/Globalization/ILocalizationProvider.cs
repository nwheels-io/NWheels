using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NWheels.Extensions;
using NWheels.Globalization.Core;

namespace NWheels.Globalization
{
    public interface ILocalizationProvider
    {
        ILocale GetDefaultLocale();
        ILocale GetCurrentLocale();
        ILocale[] GetAllSupportedLocales();
        ILocale GetLocale(CultureInfo culture);
        ILocale GetLocale(string isoCode);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ILocale
    {
        string Translate(string stringId);
        string Translate(string stringId, string origin);
        void AppendListItem(StringBuilder output, string item);
        string MakeKeyValuePair(string key, string value);
        CultureInfo Culture { get; }
        string ListSeparator { get; }
        string EqualitySign { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class VoidLocalizationProvider : ILocalizationProvider, ILocale, ICoreLocale
    {
        private readonly CultureInfo _enUS = CultureInfo.GetCultureInfo("en-US");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ILocale ILocalizationProvider.GetDefaultLocale()
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ILocale ILocalizationProvider.GetCurrentLocale()
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILocale[] GetAllSupportedLocales()
        {
            return new ILocale[] { this };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ILocale ILocalizationProvider.GetLocale(CultureInfo culture)
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILocale GetLocale(string isoCode)
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string ILocale.Translate(string stringId)
        {
            return stringId.SplitPascalCase().ConvertToPascalCase();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string ILocale.Translate(string stringId, string origin)
        {
            return stringId.SplitPascalCase().ConvertToPascalCase();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendListItem(StringBuilder output, string item)
        {
            if ( output.Length > 0 )
            {
                output.Append(", ");
            }

            output.Append(item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string MakeKeyValuePair(string key, string value)
        {
            return key + " = " + value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CultureInfo Culture
        {
            get { return _enUS; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string ILocale.ListSeparator
        {
            get { return ","; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string ILocale.EqualitySign
        {
            get { return "="; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Dictionary<string, string> ICoreLocale.GetAllTranslations(IEnumerable<LocaleEntryKey> keys, bool includeOriginFallbacks)
        {
            var result = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                result[key.ToString()] = key.StringId.SplitPascalCase();

                if (!string.IsNullOrEmpty(key.Origin) && includeOriginFallbacks)
                {
                    result[LocaleEntryKey.MakeKey(key.StringId, null)] = result[key.ToString()];
                }
            }

            return result;
        }
    }
}
