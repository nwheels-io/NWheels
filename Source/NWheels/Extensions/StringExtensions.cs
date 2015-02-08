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
    }
}
