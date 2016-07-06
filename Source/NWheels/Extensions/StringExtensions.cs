using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency.Impl;

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

        public static string ToCamelCaseExpression(this string str)
        {
            if ( string.IsNullOrEmpty(str) )
            {
                return str;
            }

            char[] resultChars = new char[str.Length];
            var atFirstLetter = true;

            for ( int i = 0 ; i < resultChars.Length ; i++ )
            {
                var c = str[i];
                resultChars[i] = (atFirstLetter ? char.ToLower(c) : c);
                atFirstLetter = (!char.IsLetterOrDigit(c) && c != '_');
            }

            return new string(resultChars);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EqualsIgnoreCase(this string s, string other)
        {
            if (s == null)
            {
                return (other == null);
            }

            return s.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ContainsIgnoreCase(this string s, string value)
        {
            if (s == null || value == null)
            {
                return false;
            }

            return s.ToLower().Contains(value.ToLower());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool StartsWithIgnoreCase(this string s, string value)
        {
            if (s == null || value == null)
            {
                return false;
            }

            return s.ToLower().StartsWith(value.ToLower());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EndsWithIgnoreCase(this string s, string value)
        {
            if (s == null || value == null)
            {
                return false;
            }

            return s.ToLower().EndsWith(value.ToLower());
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
                if ( i > 0 && (char.IsUpper(s[i]) || char.IsDigit(s[i]) != char.IsDigit(s[i-1])) )
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

        public static string OrDefaultIfNull(this string s, string defaultValue)
        {
            if (s == null)
            {
                return defaultValue;
            }
            else
            {
                return s;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string OrDefaultIfNullOrEmpty(this string s, string defaultValue)
        {
            if (string.IsNullOrEmpty(s))
            {
                return defaultValue;
            }
            else
            {
                return s;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string OrDefaultIfNullOrWhitespace(this string s, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return defaultValue;
            }
            else
            {
                return s;
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
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<string[]> TakeChunks(this IEnumerable<string> source, int maxChunkTextLength)
        {
            return new StringArrayChunkingEnumerable(source, maxChunkTextLength);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class StringArrayChunkingEnumerable : IEnumerable<string[]>
        {
            private readonly IEnumerable<string> _inner;
            private readonly int _maxTextLength;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StringArrayChunkingEnumerable(IEnumerable<string> inner, int maxTextLength)
            {
                _inner = inner;
                _maxTextLength = maxTextLength;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEnumerable

            public IEnumerator<string[]> GetEnumerator()
            {
                return new StringArrayChunkingEnumerator(_inner.GetEnumerator(), _maxTextLength);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class StringArrayChunkingEnumerator : IEnumerator<string[]>
        {
            private readonly IEnumerator<string> _inner;
            private readonly int _maxTextLength;
            private string _leftOver;
            private string[] _current;
            private bool _endOfInner;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StringArrayChunkingEnumerator(IEnumerator<string> inner, int maxTextLength)
            {
                _inner = inner;
                _maxTextLength = maxTextLength;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IDisposable

            public void Dispose()
            {
                _inner.Dispose();
                _current = null;
                _leftOver = null;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                var currentBuffer = new List<string>();
                var currentLength = 0;

                if (_leftOver != null)
                {
                    currentBuffer.Add(_leftOver);
                    currentLength += (_leftOver != null ? _leftOver.Length : 0);
                    _leftOver = null;
                }

                while (currentLength < _maxTextLength && !_endOfInner)
                {
                    if (!_inner.MoveNext())
                    {
                        _endOfInner = true;
                        break;
                    }

                    var innerCurrent = _inner.Current;
                    currentLength += (innerCurrent != null ? innerCurrent.Length : 0);

                    if (currentLength <= _maxTextLength || currentBuffer.Count == 0)
                    {
                        currentBuffer.Add(innerCurrent);
                    }
                    else
                    {
                        _leftOver = innerCurrent;
                    }
                }

                if (currentBuffer.Count > 0)
                {
                    _current = currentBuffer.ToArray();
                    return true;
                }

                _current = null;
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Reset()
            {
                _inner.Reset();
                _current = null;
                _leftOver = null;
                _endOfInner = false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string[] Current
            {
                get
                {
                    if (_current != null)
                    {
                        return _current;
                    }

                    throw new InvalidOperationException();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            #endregion
        }
    }
}
