using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NWheels.Extensions;

namespace NWheels.Globalization
{
    public interface ILocalizationProvider
    {
        ILocale GetCurrentLocale();
        ILocale GetLocale(CultureInfo culture);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ILocale
    {
        Dictionary<string, string> GetAllLocalStrings(IEnumerable<string> stringIds);
        string Translate(string stringId);
        void AppendListItem(StringBuilder output, string item);
        string MakeKeyValuePair(string key, string value);
        string ListSeparator { get; }
        string EqualitySign { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class VoidLocalizationProvider : ILocalizationProvider, ILocale
    {
        ILocale ILocalizationProvider.GetCurrentLocale()
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ILocale ILocalizationProvider.GetLocale(CultureInfo culture)
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
