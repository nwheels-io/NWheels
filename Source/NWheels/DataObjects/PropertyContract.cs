using System;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.StorageTypes;

namespace NWheels.DataObjects
{
    public static class PropertyContract
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class KeyAttribute : PropertyContractAttribute
        {
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class VersionAttribute : PropertyContractAttribute
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

        public class AccessAttribute : PropertyContractAttribute
        {
            public AccessAttribute(PropertyAccess allowedAccessFlags)
            {
                this.AllowedAccessFlags = allowedAccessFlags;
            }

            public PropertyAccess AllowedAccessFlags { get; private set; }
        }

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
                public DataTypeAttribute(Type type)
                {
                    this.Type = type;
                }
                public Type Type { get; private set; }
            }
            public class DateAttribute : DataTypeAttribute { public DateAttribute() : base(typeof(SemanticType.DefaultOf<DateTime>)) { } }
            public class TimeAttribute : DataTypeAttribute { public TimeAttribute() : base(typeof(SemanticType.DefaultOf<TimeSpan>)) { } }
            public class DurationAttribute : DataTypeAttribute { public DurationAttribute() : base(typeof(SemanticType.DefaultOf<TimeSpan>)) { } }
            public class PhoneNumberAttribute : DataTypeAttribute { public PhoneNumberAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class CurrencyAttribute : DataTypeAttribute { public CurrencyAttribute() : base(typeof(SemanticType.DefaultOf<decimal>)) { } }
            public class MultilineTextAttribute : DataTypeAttribute { public MultilineTextAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class EmailAddressAttribute : DataTypeAttribute { public EmailAddressAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class PasswordAttribute : DataTypeAttribute { public PasswordAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class UrlAttribute : DataTypeAttribute { public UrlAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class ImageUrlAttribute : DataTypeAttribute { public ImageUrlAttribute() : base(typeof(SemanticType.DefaultOf<byte[]>)) { } }
            public class CreditCardAttribute : DataTypeAttribute { public CreditCardAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class PostalCodeAttribute : DataTypeAttribute { public PostalCodeAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class UploadAttribute : DataTypeAttribute { public UploadAttribute() : base(typeof(SemanticType.DefaultOf<byte[]>)) { } }
            public class HtmlAttribute : DataTypeAttribute { public HtmlAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class XmlAttribute : DataTypeAttribute { public XmlAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
            public class JsonAttribute : DataTypeAttribute { public JsonAttribute() : base(typeof(SemanticType.DefaultOf<string>)) { } }
        }

        public static class Storage
        {
            public class StorageTypeAttribute : PropertyContractAttribute
            {
                public StorageTypeAttribute(Type type)
                {
                    this.Type = type;
                }
                public Type Type { get; private set; }
            }
            public class JsonAttribute : StorageTypeAttribute { public JsonAttribute() : base(typeof(JsonStorageType<>)) { } }

            public class RelationalMappingAttribute : PropertyContractAttribute
            {
                public void ApplyTo(PropertyMetadataBuilder metadata)
                {
                    var mapping = metadata.SafeGetRelationalMapping();

                    if ( !string.IsNullOrWhiteSpace(Table) )
                    {
                        mapping.TableName = Table;
                    }
                    if ( !string.IsNullOrWhiteSpace(Column) )
                    {
                        mapping.ColumnName = Column;
                    }
                    if ( !string.IsNullOrWhiteSpace(ColumnType) )
                    {
                        mapping.ColumnType = ColumnType;
                    }
                }
                public string Table { get; set; }
                public string Column { get; set; }
                public string ColumnType { get; set; }
            }
        } 

        public static class Validation
        {
            public class ValidatorAttribute : PropertyContractAttribute
            {
                public ValidatorAttribute(Type validatorType)
                {
                    this.ValidatorType = validatorType;
                }
                public Type ValidatorType { get; private set; }
            }

            public class MinValueAttribute : PropertyContractAttribute
            {
                public MinValueAttribute(object value)
                {
                    this.Value = value;
                }
                public object Value { get; private set; }
            }
            public class MaxValueAttribute : PropertyContractAttribute
            {
                public MaxValueAttribute(object value)
                {
                    this.Value = value;
                }
                public object Value { get; private set; }
            }
            public class RangeAttribute : PropertyContractAttribute
            {
                public RangeAttribute(object min, object max)
                {
                    this.Min = min;
                    this.Max = max;
                }
                public object Min { get; set; }
                public object Max { get; set; }
                public bool MinExclusive { get; set; }
                public bool MaxExclusive { get; set; }
            }
            public class LengthAttribute : PropertyContractAttribute
            {
                public LengthAttribute(int min, int max)
                {
                    this.Min = min;
                    this.Max = max;
                }
                public int Min { get; private set; }
                public int Max { get; private set; }
            }
            public class MaxLengthAttribute : PropertyContractAttribute
            {
                public MaxLengthAttribute(int maxLength)
                {
                    this.MaxLength = maxLength;
                }
                public int MaxLength { get; private set; }
            }
            public class MinLengthAttribute : PropertyContractAttribute
            {
                public MinLengthAttribute(int minLength)
                {
                    this.MinLength = minLength;
                }
                public int MinLength { get; private set; }
            }
            public class RegularExpressionAttribute : PropertyContractAttribute
            {
                public RegularExpressionAttribute(string expression)
                {
                    this.Expression = expression;
                }
                public string Expression { get; private set; }
            }
            public class FutureAttribute : PropertyContractAttribute
            {
                public FutureAttribute(string lagTimeSpan)
                {
                    this.Lag = TimeSpan.Parse(lagTimeSpan);
                }
                public TimeSpan Lag { get; set; }
            }
            public class PastAttribute : PropertyContractAttribute
            {
                public PastAttribute(string lagTimeSpan)
                {
                    this.Lag = TimeSpan.Parse(lagTimeSpan);
                }
                public TimeSpan Lag { get; set; }
            }
        }

        public static class Presentation
        {
            public class DisplayNameAttribute : PropertyContractAttribute
            {
                public DisplayNameAttribute(string text)
                {
                    this.Text = text;
                }
                public string Text { get; private set; }
            }
            public class DisplayFormatAttribute : PropertyContractAttribute
            {
                public DisplayFormatAttribute(string format)
                {
                    this.Format = format;
                }
                public string Format { get; private set; }
            }
            public class SortAttribute : PropertyContractAttribute
            {
                public SortAttribute(bool ascending)
                {
                    this.Ascending = ascending;
                }
                public bool Ascending { get; private set; }
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