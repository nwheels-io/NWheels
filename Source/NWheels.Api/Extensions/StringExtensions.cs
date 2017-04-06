using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Extensions
{
    public static class StringExtensions
    {
        public static string TrimPrefix(this string source, string prefix)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(prefix))
            {
                return source;
            }

            if (source.Length <= prefix.Length || !source.StartsWith(prefix))
            {
                return source;
            }

            return source.Substring(startIndex: prefix.Length);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string TrimSuffix(this string source, string suffix)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(suffix))
            {
                return source;
            }

            if (source.Length <= suffix.Length || !source.EndsWith(suffix))
            {
                return source;
            }

            return source.Substring(startIndex: 0, length: source.Length - suffix.Length);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string DefaultIfNullOrEmpty(this string source, string defaultValue)
        {
            if (string.IsNullOrEmpty(source))
            {
                return defaultValue;
            }

            return source;
        }
    }
}
