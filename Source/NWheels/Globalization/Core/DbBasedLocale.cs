using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NWheels.Extensions;

namespace NWheels.Globalization.Core
{
    public class DbBasedLocale : ILocale, ICoreLocale
    {
        private readonly Dictionary<string, string> _translations;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DbBasedLocale(IApplicationLocaleEntity localeEntity)
        {
            var cultureName = (string.IsNullOrWhiteSpace(localeEntity.CultureCode) ? localeEntity.IsoCode : localeEntity.CultureCode);
            
            this.Culture = CultureInfo.GetCultureInfo(cultureName);
            this.ListSeparator = ",";
            this.EqualitySign = "=";

            _translations = new Dictionary<string, string>(localeEntity.Entries);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ILocale

        public Dictionary<string, string> GetAllLocalStrings(IEnumerable<string> stringIds)
        {
            return stringIds
                .Select(id => new KeyValuePair<string, string>(id, _translations.GetValueOrDefault(id, id)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string Translate(string stringId)
        {
            return _translations.GetValueOrDefault(stringId, stringId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendListItem(StringBuilder output, string item)
        {
            if (output.Length > 0)
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

        public CultureInfo Culture { get; private set; }
        public string ListSeparator { get; private set; }
        public string EqualitySign { get; private set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ICoreLocale

        public virtual void SetLocalStrings(Dictionary<string, string> localStringByStringId)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
