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

            _translations = new Dictionary<string, string>();

            foreach (var entry in localeEntity.Entries)
            {
                _translations[LocaleEntryKey.MakeKey(entry.StringId, entry.Origin)] = entry.Translation;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ILocale

        public string Translate(string stringId)
        {
            var key = LocaleEntryKey.MakeKey(stringId, null);
            return _translations.GetValueOrDefault(key, defaultValue: stringId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Translate(string stringId, string origin)
        {
            var key = LocaleEntryKey.MakeKey(stringId, origin);
            string translation;

            if (_translations.TryGetValue(key, out translation))
            {
                return translation;
            }

            if (string.IsNullOrEmpty(origin))
            {
                return stringId;
            }

            var fallbackKey = LocaleEntryKey.MakeKey(stringId, null);
            return _translations.GetValueOrDefault(fallbackKey, defaultValue: stringId);
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

        public Dictionary<string, string> GetAllTranslations(IEnumerable<LocaleEntryKey> keys, bool includeOriginFallbacks = true)
        {
            var result = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                string translation = null;
                string originFallbackKey = null;
                string originFallbackTranslation = null;
                
                _translations.TryGetValue(key.ToString(), out translation);

                if ((translation == null || includeOriginFallbacks) && !string.IsNullOrEmpty(key.Origin))
                {
                    originFallbackKey = LocaleEntryKey.MakeKey(key.StringId, null);
                    _translations.TryGetValue(originFallbackKey, out originFallbackTranslation);
                }

                if (!includeOriginFallbacks)
                {
                    result[key.ToString()] = translation ?? originFallbackTranslation;
                }

                if (includeOriginFallbacks && originFallbackTranslation != null)
                {
                    result[originFallbackKey] = originFallbackTranslation;
                }
            }

            return result;
        }

        #endregion
    }
}
