using System;

namespace NWheels.DataObjects
{
    public static class PropertyContract
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class KeyAttribute : PropertyContractAttribute
        {
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class RequiredAttribute : PropertyContractAttribute
        {
            public bool AllowEmpty { get; set; }
        }

        public class ReadOnlyAttribute : PropertyContractAttribute { }

        public class WriteOnlyAttribute : PropertyContractAttribute { }

        public class SearchOnlyAttribute : PropertyContractAttribute { }

        public class UniqueAttribute : PropertyContractAttribute { }

        public class DefaultValueAttribute : PropertyContractAttribute
        {
            public DefaultValueAttribute(object value)
            {
                this.Value = value;
            }

            public object Value { get; private set; }
        }
        
        public static class Semantic
        {
            public class DataTypeAttribute : PropertyContractAttribute
            {
                public DataTypeAttribute()
                {
                }
                public DataTypeAttribute(Type semanticDataType)
                {
                }
            }

            public class DateAttribute : DataTypeAttribute { }
            public class TimeAttribute : DataTypeAttribute { }
            public class DurationAttribute : DataTypeAttribute { }
            public class PhoneNumberAttribute : DataTypeAttribute { }
            public class CurrencyAttribute : DataTypeAttribute { }
            public class MultilineTextAttribute : DataTypeAttribute { }
            public class EmailAddressAttribute : DataTypeAttribute { }
            public class PasswordAttribute : DataTypeAttribute { }
            public class UrlAttribute : DataTypeAttribute { }
            public class ImageUrlAttribute : DataTypeAttribute { }
            public class CreditCardAttribute : DataTypeAttribute { }
            public class PostalCodeAttribute : DataTypeAttribute { }
            public class UploadAttribute : DataTypeAttribute { }
            public class HtmlAttribute : DataTypeAttribute { }
            public class XmlAttribute : DataTypeAttribute { }
            public class JsonAttribute : DataTypeAttribute { }
        }

        public static class Validation
        {
            public class ValidatorAttribute : PropertyContractAttribute
            {
                public ValidatorAttribute(Type validatorType)
                {
                }
            }

            public class MinValueAttribute : PropertyContractAttribute
            {
                public MinValueAttribute(object value) { }
            }
            public class MaxValueAttribute : PropertyContractAttribute
            {
                public MaxValueAttribute(object value){ }
            }
            public class RangeAttribute : PropertyContractAttribute
            {
                public RangeAttribute(object min = null, object max = null) { }
                public bool MinExclusive { get; set; }
                public bool MaxExclusive { get; set; }
            }
            public class LengthAttribute : PropertyContractAttribute
            {
                public LengthAttribute(int min, int max) { }
            }
            public class MaxLengthAttribute : PropertyContractAttribute
            {
                public MaxLengthAttribute(int maxLength) { }
            }
            public class MinLengthAttribute : PropertyContractAttribute
            {
                public MinLengthAttribute(int minLength) { }
            }
            public class RegularExpressionAttribute : PropertyContractAttribute
            {
                public RegularExpressionAttribute(string regex) { }
            }
            public class FutureAttribute : PropertyContractAttribute
            {
                public TimeSpan NowPlus { get; set; }
            }
            public class PastAttribute : PropertyContractAttribute
            {
                public TimeSpan NowMinus { get; set; }
            }
        }

        public static class Presentation
        {
            public class DisplayNameAttribute : PropertyContractAttribute
            {
                public DisplayNameAttribute(string text)
                {
                }
            }
            public class DisplayFormatAttribute : PropertyContractAttribute
            {
                public DisplayFormatAttribute(string format)
                {
                }
            }
            public class SortAttribute : PropertyContractAttribute
            {
                public SortAttribute(bool ascending)
                {
                }
            }
        }

        public static class Relation
        {
            public class OneToOneAttribute : PropertyContractAttribute { }
            public class OneToManyAttribute : PropertyContractAttribute { }
            public class ManyToOneAttribute : PropertyContractAttribute { }
            public class ManyToManyAttribute : PropertyContractAttribute { }
        }

        public static class Security
        {
            public class SensitiveAttribute : PropertyContractAttribute { }
        }
    }
}