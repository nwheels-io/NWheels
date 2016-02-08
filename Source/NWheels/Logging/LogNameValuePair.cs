using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using NWheels.Extensions;

namespace NWheels.Logging
{
    public interface ILogNameValuePair
    {
        string FormatName();
        string FormatValue();
        string FormatLogString();
        LogContentTypes GetContentTypes();
        bool IsBaseValue();
        bool IsIndexed();
        bool IsIncludedInSingleLineText();
        object GetValueAsObject();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct LogNameValuePair<T> : ILogNameValuePair
    {
        public string Name;
        public T Value;
        public string Format;
        public bool IsDetail;
        public bool IsIndexed;
        public int MaxStringLength;
        public LogContentTypes ContentTypes;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Pure]
        public string FormatName()
        {
            return this.Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Pure]
        public string FormatValue()
        {
            var formattable = Value as IFormattable;

            if ( Format != null && formattable != null )
            {
                return formattable.ToString(Format, CultureInfo.CurrentCulture);
            }
            else if ( !typeof(T).IsValueType && (object)Value == null )
            {
                return "null";
            }
            else
            {
                return Value.ToString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Pure]
        public string FormatLogString()
        {
            var unprefixedName = UnprefixBaseName(this.Name);
            var namePart = (
                !string.IsNullOrEmpty(unprefixedName) ?
                unprefixedName.TruncateAt(50) + "=" :
                string.Empty);

            var valueMaxLength = (this.MaxStringLength > 0 ? this.MaxStringLength : 255);
            var valuePart = (FormatValue() ?? string.Empty).TruncateAt(valueMaxLength).Replace('"', '\'');

            if ( unprefixedName != null && valuePart.Any(c => char.IsWhiteSpace(c) || c == '=') )
            {
                valuePart = "\"" + valuePart + "\"";
            }

            return namePart + valuePart;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsBaseValue()
        {
            return (this.Name != null && this.Name.StartsWith("$"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool ILogNameValuePair.IsIndexed()
        {
            return this.IsIndexed;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsIncludedInSingleLineText()
        {
            return (!this.IsDetail && !this.IsBaseValue());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogContentTypes GetContentTypes()
        {
            return this.ContentTypes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object GetValueAsObject()
        {
            return this.Value;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string UnprefixBaseName(string name)
        {
            if ( name != null )
            {
                if ( name.Length >= 2 && name[0] == '$' && name[1] == '$' )
                {
                    return null;
                }
                else if ( name.Length >= 1 && name[0] == '$' )
                {
                    return name.Substring(1);
                }
            }

            return name;
        }
    }
}