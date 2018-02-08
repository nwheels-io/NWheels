using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NWheels.Kernel.Api.Extensions
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToPathString(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            return source
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string PascalCaseToHumanReadableText(this string s, char delimiter = ' ')
        {
            if (s == null)
            {
                return null;
            }

            var length = s.Length;

            if (length <= 1)
            {
                return s;
            }

            var output = new StringBuilder(length * 2);

            output.Append(s[0]);

            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsDigit(s[i]))
                {
                    var isNewWord = !char.IsDigit(s[i - 1]);

                    if (isNewWord)
                    {
                        output.Append(delimiter);
                    }
                }
                else if (char.IsUpper(s[i]))
                {
                    var isNewWord = (!char.IsUpper(s[i - 1]) || (i < s.Length - 1 && char.IsLower(s[i + 1])));

                    if (isNewWord)
                    {
                        output.Append(delimiter);
                        output.Append((i < s.Length - 1 && char.IsUpper(s[i + 1])) ? s[i] : char.ToLower(s[i]));
                        continue;
                    }
                }

                output.Append(s[i]);
            }

            return output.ToString();
        }
    }
}
