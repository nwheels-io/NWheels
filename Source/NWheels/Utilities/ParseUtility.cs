using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Utilities
{
    public static class ParseUtility
    {
        private static readonly Dictionary<Type, Delegate> s_ParsersByType = new Dictionary<Type, Delegate> {
            { typeof(string), new Func<string, string>(s => s) },
            { typeof(bool), new Func<string, bool>(s => Boolean.Parse(s)) },
            { typeof(Int32), new Func<string, Int32>(Int32.Parse) },
            { typeof(Int64), new Func<string, Int64>(Int64.Parse) },
            { typeof(Guid), new Func<string, Guid>(Guid.Parse) },
            { typeof(decimal), new Func<string, decimal>(Decimal.Parse) },
            { typeof(Type), new Func<string, Type>(s => Type.GetType(s, throwOnError: true)) },
            { typeof(DateTime), new Func<string, DateTime>(s => DateTime
                .ParseExact(s, new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss" }, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal)
                .ToUniversalTime()) },
            { typeof(TimeSpan), new Func<string, TimeSpan>(TimeSpan.Parse) }

        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, Func<string, object>> s_NonTypedParsersByType = new Dictionary<Type, Func<string, object>> {
            { typeof(string), s => s },
            { typeof(bool), s => Boolean.Parse(s) },
            { typeof(Int32), s => Int32.Parse(s) },
            { typeof(Int64), s => Int64.Parse(s) },
            { typeof(Guid), s => Guid.Parse(s) },
            { typeof(decimal), s => Decimal.Parse(s) },
            { typeof(Type), s => Type.GetType(s, throwOnError: true) },
            { typeof(DateTime), s => DateTime
                .ParseExact(s, new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss" }, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal)
                .ToUniversalTime() },
            { typeof(TimeSpan), s => TimeSpan.Parse(s) }
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T Parse<T>(string s)
        {
            if ( typeof(T).IsEnum )
            {
                return (T)Enum.Parse(typeof(T), s, ignoreCase: true);
            }
            else
            {
                Func<string, T> parser = (Func<string, T>)s_ParsersByType[typeof(T)];
                return parser(s);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object Parse(string s, Type asType)
        {
            if ( asType.IsEnum )
            {
                return Enum.Parse(asType, s, ignoreCase: true);
            }
            else
            {
                return s_NonTypedParsersByType[asType](s);
            }
        }

        public static DateTime TryParse(string s, DateTime defaultValue)
        {
            if ( string.IsNullOrEmpty(s) )
                return defaultValue;

            DateTime value;
            if (DateTime.TryParse(s, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
