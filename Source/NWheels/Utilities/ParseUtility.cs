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
            { typeof(Int32), new Func<string, Int32>(Int32.Parse) },
            { typeof(Int64), new Func<string, Int64>(Int64.Parse) },
            { typeof(Guid), new Func<string, Guid>(Guid.Parse) },
            { typeof(decimal), new Func<string, decimal>(Decimal.Parse) },
            { typeof(Type), new Func<string, Type>(s => Type.GetType(s, throwOnError: true)) },
            { typeof(DateTime), new Func<string, DateTime>(s => DateTime.ParseExact(
                s, 
                new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss" },
                CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal)) },
            { typeof(TimeSpan), new Func<string, TimeSpan>(TimeSpan.Parse) }

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
    }
}
