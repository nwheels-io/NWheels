using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NWheels.Extensions;

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
        Dictionary<string, string> GetAllLocalStrings(IEnumerable<string> stringIds);
        string Translate(string stringId);
        void AppendListItem(StringBuilder output, string item);
        string MakeKeyValuePair(string key, string value);
        CultureInfo Culture { get; }
        string ListSeparator { get; }
        string EqualitySign { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class VoidLocalizationProvider : ILocalizationProvider, ILocale
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

        public Dictionary<string, string> GetLocalStrings(IEnumerable<string> stringIds, CultureInfo culture)
        {
            return stringIds.ToDictionary(s => s, s => s.SplitPascalCase().ConvertToPascalCase());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Dictionary<string, string> ILocale.GetAllLocalStrings(IEnumerable<string> stringIds)
        {
            return stringIds.ToDictionary(s => s, s => s.SplitPascalCase().ConvertToPascalCase());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string ILocale.Translate(string stringId)
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
    }
}
