using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels
{
    namespace I18n
    {
        public interface ILocalizationService
        {
            string GetLocalDisplayString<T>(T value, string formatPattern);
            string GetLocalDisplayString<T>(T value, Attribute memberContract);
        }

        public static class TypeContract
        {
            public class LocalizablesAttribute : Attribute
            {
                public string DefaultCulture { get; set; }
            }
        }

        public static class MemberContract
        {
            public class InDefaultCulture : Attribute
            {
                public InDefaultCulture(object cultureSpecificvalue)
                {
                }
            }
            public class FormatDataSourceAttribute : Attribute
            {
                public FormatDataSourceAttribute(Type dataSourceType)
                {
                }
            }
            public class CultureScopeMethod : Attribute
            {
            }
        }
    }
}
