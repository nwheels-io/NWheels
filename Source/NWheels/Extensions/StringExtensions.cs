using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string s, string other)
        {
            return s.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string FormatIf(this string format, params object[] args)
        {
            if ( args != null && args.Length > 0 )
            {
                return string.Format(format, args);
            }
            else
            {
                return format;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string SplitPascalCase(this string s, char delimiter = ' ')
        {
            var output = new StringBuilder(s.Length * 2);

            for ( int i = 0 ; i < s.Length ; i++ )
            {
                if ( i > 0 && char.IsUpper(s[i]) )
                {
                    output.Append(delimiter);
                    output.Append(char.ToLower(s[i]));
                }
                else
                {
                    output.Append(s[i]);
                }
            }

            return output.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string TruncateAt(this string s, int maxLength)
        {
            if ( s == null || s.Length <= maxLength )
            {
                return s;
            }
            else
            {
                return s.Substring(0, maxLength);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string EmptyIfNull(this string s)
        {
            if ( s == null )
            {
                return string.Empty;
            }
            else
            {
                return s;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string NullIfEmpty(this string s)
        {
            if ( string.IsNullOrEmpty(s) )
            {
                return null;
            }
            else
            {
                return s;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string NullIfEmptyOrWhitespace(this string s)
        {
            if ( string.IsNullOrWhiteSpace(s) )
            {
                return null;
            }
            else
            {
                return s;
            }
        }
    }
}
