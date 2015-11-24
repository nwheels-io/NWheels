using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;

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

        private static readonly Dictionary<Type, object> s_NullableNullValues = new Dictionary<Type, object> {
            { typeof(bool), new Nullable<bool>() },
            { typeof(Int32), new Nullable<int>() },
            { typeof(Int64), new Nullable<long>() },
            { typeof(Guid), new Nullable<Guid>() },
            { typeof(decimal), new Nullable<decimal>() },
            { typeof(DateTime), new Nullable<DateTime>() },
            { typeof(TimeSpan), new Nullable<TimeSpan>() }
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T Parse<T>(string s)
        {
            if ( typeof(T).IsEnum )
            {
                return (T)Enum.Parse(typeof(T), s, ignoreCase: true);
            }

            Delegate parser;

            if ( s_ParsersByType.TryGetValue(typeof(T), out parser) )
            {
                return ((Func<string, T>)parser)(s);
            }

            if ( typeof(T).IsNullableValueType() )
            {
                if ( string.IsNullOrEmpty(s) )
                {
                    return default(T);
                }
                else
                {
                    return (T)Parse(s, typeof(T));
                }
            }

            throw new NotSupportedException("ParseUtility does not support type: " + typeof(T).FullName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool TryParse<T>(string s, out T value)
        {
            try
            {
                value = Parse<T>(s);
                return true;
            }
            catch ( Exception )
            {
                value = default(T);
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object Parse(string s, Type asType)
        {
            if ( asType.IsNullableValueType() )
            {
                if ( string.IsNullOrEmpty(s) )
                {
                    return GetNullableNullValue(asType);
                }
                else
                {
                    asType = asType.GetGenericArguments()[0];
                }
            }

            if ( asType.IsEnum )
            {
                return Enum.Parse(asType, s, ignoreCase: true);
            }
            else
            {
                return s_NonTypedParsersByType[asType](s);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool TryParse(string s, Type asType, out object value)
        {
            value = null;

            try
            {
                value = Parse(s, asType);
                return true;
            }
            catch ( Exception )
            {
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DateTime TryParse(string s, DateTime defaultValue)
        {
            if ( string.IsNullOrEmpty(s) )
            {
                return defaultValue;
            }

            DateTime value;
            
            if ( DateTime.TryParse(s, out value) )
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static object GetNullableNullValue(Type asType)
        {
            object nullValue;

            if ( s_NullableNullValues.TryGetValue(asType.UnderlyingSystemType, out nullValue) )
            {
                return nullValue;
            }
            else
            {
                return Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(asType.GetGenericArguments()[0]));
            }
        }
    }
}
