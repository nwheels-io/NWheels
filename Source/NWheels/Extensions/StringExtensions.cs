using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class StringExtensions
    {
        public static string TrimLead(this string str, string prefix)
        {
            if ( str != null && prefix != null && str.StartsWith(prefix) )
            {
                return str.Substring(prefix.Length);
            }
            else
            {
                return str;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string TrimTail(this string str, string suffix)
        {
            if ( str != null && suffix != null && str.EndsWith(suffix) )
            {
                return str.Substring(0, str.Length - suffix.Length);
            }
            else
            {
                return str;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ConvertToPascalCase(this string str)
        {
            if ( !string.IsNullOrEmpty(str) )
            {
                return str.Substring(0, 1).ToUpper() + str.Substring(1);
            }
            else
            {
                return str;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ConvertToCamelCase(this string str)
        {
            if ( !string.IsNullOrEmpty(str) )
            {
                return str.Substring(0, 1).ToLower() + str.Substring(1);
            }
            else
            {
                return str;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string AddEnglishVerbIngSuffix(this string s)
        {
            if ( s == null || s.Length < 2 )
            {
                return s;
            }

            var length = s.Length;
            var isLower = char.IsLower(s[length - 1]);
            var lastChar = char.ToLower(s[length - 1]);
            var suffix = (isLower ? "ing" : "ING");

            if ( lastChar == 'e' )
            {
                if ( s[length - 2] != 'i' )
                {
                    return s.Substring(0, length - 1) + suffix;
                }
                else
                {
                    return s.Substring(0, length - 2) + (isLower ? 'y' : 'Y') + suffix;
                }
            }

            if ( lastChar == 'w' || lastChar == 'x' || lastChar == 'y' )
            {
                return s + suffix;
            }

            if ( length >= 3 )
            {
                var endsWithConsonantVowelConsonant = (s[length-1].IsEnglishConsonant() && s[length-2].IsEnglishVowel() && s[length-3].IsEnglishConsonant());

                if ( endsWithConsonantVowelConsonant )
                {
                    var vowelCount = s.Count(CharExtensions.IsEnglishVowel);

                    if ( vowelCount < 2 )
                    {
                        return s + s[length - 1] + suffix;
                    }
                }
            }

            return s + suffix;
        }
    }
}
