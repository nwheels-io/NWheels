using System;
using System.Linq;
using System.Xml.Linq;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.TypeModel;
using NWheels.TypeModel.Core.StorageTypes;

namespace NWheels.DataObjects
{
    public static class PropertyContract
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public abstract class KeyAttribute : PropertyContractAttribute
        {
            protected KeyAttribute(KeyKind kind)
            {
                this.Kind = kind;
            }
            public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
            {
                base.ApplyTo(property, cache);
                var key = new KeyMetadataBuilder() {
                    Kind = this.Kind
                };
                key.Properties.Add(property);
                ConfigureKey(key, property, cache);
                property.DeclaringContract.AllKeys.Add(key);
            }
            public KeyKind Kind { get; private set; }
            protected abstract void ConfigureKey(KeyMetadataBuilder key, PropertyMetadataBuilder property, TypeMetadataCache cache);
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class SearchKeyAttribute : KeyAttribute
        {
            public SearchKeyAttribute() : base(KeyKind.Index)
            {
            }
            protected override void ConfigureKey(KeyMetadataBuilder key, PropertyMetadataBuilder property, TypeMetadataCache cache)
            {
                key.Name = "SK_" + property.Name;
            }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class EntityIdAttribute : KeyAttribute
        {
            public EntityIdAttribute() : base(KeyKind.Primary)
            {
            }
            public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
            {
                if ( property.DeclaringContract.PrimaryKey == null )
                {
                    base.ApplyTo(property, cache);
                    property.Kind = PropertyKind.Scalar;
                    property.Role = PropertyRole.Key;
                    property.Validation.IsRequired = true;
                }
            }
            protected override void ConfigureKey(KeyMetadataBuilder key, PropertyMetadataBuilder property, TypeMetadataCache cache)
            {
                key.Name = "PK_" + property.DeclaringContract.Name;
                property.DeclaringContract.PrimaryKey = key;
            }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class VersionAttribute : PropertyContractAttribute
        {
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
        public class RequiredAttribute : PropertyContractAttribute
        {
            public bool AllowEmpty { get; set; }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class CalculatedAttribute : PropertyContractAttribute
        {
            #region Overrides of PropertyContractAttribute

            public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
            {
                property.IsCalculated = true;
            }

            #endregion
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

        public class UniquePerParentAttribute : PropertyContractAttribute { }

        public class DefaultValueAttribute : PropertyContractAttribute
        {
            public DefaultValueAttribute(object value)
            {
                this.Value = value;
            }

            public object Value { get; private set; }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class NumericPrecisionAttribute : PropertyContractAttribute
        {
            public NumericPrecisionAttribute(int decimalDigits)
            {
                this.DecimalDigits = decimalDigits;
            }

            public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
            {
                property.NumericPrecision = this.DecimalDigits;
            }

            public int DecimalDigits { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class AutoGenerateAttribute : PropertyContractAttribute
        {
            public AutoGenerateAttribute(Type valueGeneratorType)
            {
                this.ValueGeneratorType = valueGeneratorType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ValueGeneratorType { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PartitionAttribute : PropertyContractAttribute
        {
            #region Overrides of PropertyContractAttribute

            public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
            {
                property.IsPartition = true;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string PartitionNameProperty { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Semantic
        {
            public abstract class SemanticAttributeBase : PropertyContractAttribute
            {
                private readonly SemanticType.SemanticDataTypeBuilder _semanticDataType;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                protected SemanticAttributeBase(
                    Type dataType,
                    WellKnownSemanticType wellKnownSemantic,
                    Action<SemanticType.SemanticDataTypeBuilder> defaultConfiguration = null)
                    : this(wellKnownSemantic.ToString(), dataType, wellKnownSemantic, defaultConfiguration)
                {
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                protected SemanticAttributeBase(
                    string name,
                    Type dataType,
                    WellKnownSemanticType wellKnownSemantic = WellKnownSemanticType.None, 
                    Action<SemanticType.SemanticDataTypeBuilder> defaultConfiguration = null)
                {
                    _semanticDataType = SemanticType.SemanticDataTypeBuilder.Create(name, dataType);
                    _semanticDataType.WellKnownSemantic = wellKnownSemantic;

                    if ( defaultConfiguration != null )
                    {
                        defaultConfiguration(_semanticDataType);
                    }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                #region Overrides of PropertyContractAttribute

                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    property.SemanticType = _semanticDataType;
                    
                    OnConfigurePropertySemantics(cache, property, _semanticDataType);
                    
                    property.Validation.MergeWith(_semanticDataType.DefaultValidation);
                }

                #endregion

                //---------------------------------------------------------------------------------------------------------------------------------------------

                protected virtual void OnConfigurePropertySemantics(
                    TypeMetadataCache cache,
                    PropertyMetadataBuilder property, 
                    SemanticType.SemanticDataTypeBuilder semantics)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class InheritorOfAttribute : SemanticAttributeBase
            {
                public InheritorOfAttribute(Type requiredBase) : base("InheritorOf", typeof(Type), WellKnownSemanticType.None)
                {
                    this.RequiredBase = requiredBase;
                }
                public Type RequiredBase { get; private set; }
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    property.Validation.AncestorClrType = RequiredBase;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class CreaditCardAttribute : SemanticAttributeBase
            {
                public CreaditCardAttribute()
                    : base(typeof(DateTime), WellKnownSemanticType.CreditCard)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class CultureAttribute : SemanticAttributeBase
            {
                public CultureAttribute()
                    : base(typeof(string), WellKnownSemanticType.Culture)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class CurrencyAttribute : SemanticAttributeBase
            {
                public CurrencyAttribute()
                    : base(typeof(decimal), WellKnownSemanticType.Currency)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DateAttribute : SemanticAttributeBase
            {
                public DateAttribute(TimeUnits units = TimeUnits.YearMonthDay) 
                    : base(typeof(DateTime), WellKnownSemanticType.Date, sem => sem.TimeUnits = units)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DurationAttribute : SemanticAttributeBase
            {
                public DurationAttribute(TimeUnits units = TimeUnits.HourMinuteSecond)
                    : base(typeof(TimeSpan), WellKnownSemanticType.Duration, sem => sem.TimeUnits = units)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class EmailAddressAttribute : SemanticAttributeBase
            {
                public EmailAddressAttribute()
                    : base(typeof(string), WellKnownSemanticType.EmailAddress)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class IPAddressAttribute : SemanticAttributeBase
            {
                public IPAddressAttribute()
                    : base(typeof(string), WellKnownSemanticType.IPAddress)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class FileUploadAttribute : SemanticAttributeBase
            {
                public FileUploadAttribute()
                    : base(typeof(byte[]), WellKnownSemanticType.FileUpload)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class ImageUploadAttribute : SemanticAttributeBase
            {
                public ImageUploadAttribute()
                    : base(typeof(byte[]), WellKnownSemanticType.ImageUpload)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class ImageUrlAttribute : SemanticAttributeBase
            {
                public ImageUrlAttribute()
                    : base(typeof(string), WellKnownSemanticType.ImageUrl)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class JsonAttribute : SemanticAttributeBase
            {
                public JsonAttribute()
                    : base(typeof(string), WellKnownSemanticType.Json)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class LoginNameAttribute : SemanticAttributeBase
            {
                public LoginNameAttribute()
                    : base(typeof(string), WellKnownSemanticType.LoginName, sem => {
                        sem.DefaultValidation.IsUnique = true;
                        sem.DefaultValidation.MinLength = 5;
                        sem.DefaultValidation.MaxLength = 100;
                    })
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class LocalFilePathAttribute : SemanticAttributeBase
            {
                public LocalFilePathAttribute()
                    : base(typeof(string), WellKnownSemanticType.LocalFilePath)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class PasswordAttribute : SemanticAttributeBase
            {
                public PasswordAttribute()
                    : base(typeof(string), WellKnownSemanticType.Password, sem => { 
                        sem.DefaultValidation.MinLength = 5;
                        sem.DefaultValidation.MaxLength = 20;
                    })
                {
                }
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    base.ApplyTo(property, cache);
                    property.IsSensitive = true;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class PercentageAttribute : SemanticAttributeBase
            {
                public PercentageAttribute()
                    : base(typeof(decimal), WellKnownSemanticType.Percentage)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class PhoneNumberAttribute : SemanticAttributeBase
            {
                public PhoneNumberAttribute()
                    : base(typeof(string), WellKnownSemanticType.PhoneNumber)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class PostalCodeAttribute : SemanticAttributeBase
            {
                public PostalCodeAttribute()
                    : base(typeof(string), WellKnownSemanticType.PostalCode)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class ProfilePhotoAttribute : SemanticAttributeBase
            {
                public ProfilePhotoAttribute()
                    : base(typeof(byte[]), WellKnownSemanticType.ProfilePhoto)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class TimeAttribute : SemanticAttributeBase
            {
                public TimeAttribute(TimeUnits units = TimeUnits.HourMinuteSecond)
                    : base(typeof(DateTime), WellKnownSemanticType.Time, sem => sem.TimeUnits = units)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class UrlAttribute : SemanticAttributeBase
            {
                public UrlAttribute()
                    : base(typeof(string), WellKnownSemanticType.Url)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class XmlAttribute : SemanticAttributeBase
            {
                public XmlAttribute()
                    : base(typeof(string), WellKnownSemanticType.Xml)
                {
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class DisplayNameAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    property.DeclaringContract.DefaultDisplayProperties.Add(property);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class OrderByAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    property.DeclaringContract.DefaultSortProperties.Add(property);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Aggregation
        {
            public class Dimension : PropertyContractAttribute { }
            public class Measure : PropertyContractAttribute { }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
            public class ClrTypeAttribute : StorageTypeAttribute { public ClrTypeAttribute() : base(typeof(ClrTypeStorageType)) { } }
            public class MoneyAttribute : StorageTypeAttribute { public MoneyAttribute() : base(typeof(MoneyStorageType)) { } }

            public class DecimalAsIntSixDecimalDigitsAttribute : StorageTypeAttribute
            {
                public DecimalAsIntSixDecimalDigitsAttribute()
                    : base(typeof(DecimalAsInt64StorageType))
                {
                }
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    base.ApplyTo(property, cache);
                    property.NumericPrecision = 6;
                }
            }

            public class RelationalMappingAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    var mapping = property.SafeGetRelationalMapping();

                    if (!string.IsNullOrWhiteSpace(Table))
                    {
                        mapping.TableName = Table;
                    }
                    if (!string.IsNullOrWhiteSpace(Column))
                    {
                        mapping.ColumnName = Column;
                    }
                    if (!string.IsNullOrWhiteSpace(ColumnType))
                    {
                        mapping.ColumnType = ColumnType;
                    }
                    if (StorageStyle != PropertyStorageStyle.Undefined)
                    {
                        mapping.StorageStyle = this.StorageStyle;
                    }
                }
             
                public string Table { get; set; }
                public string Column { get; set; }
                public string ColumnType { get; set; }
                public PropertyStorageStyle StorageStyle { get; set; }
            }

            public class EmbeddedInParentAttribute : PropertyContractAttribute
            {
                public EmbeddedInParentAttribute()
                {
                    this.IsEmbedded = true;
                }
                public EmbeddedInParentAttribute(bool isEmbedded)
                {
                    this.IsEmbedded = isEmbedded;
                }
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    var mapping = property.SafeGetRelationalMapping();

                    if ( IsEmbedded )
                    {
                        mapping.StorageStyle = (property.IsCollection ? PropertyStorageStyle.EmbeddedObjectCollection : PropertyStorageStyle.EmbeddedObject);
                    }
                    else
                    {
                        mapping.StorageStyle = (property.IsCollection ? PropertyStorageStyle.InverseForeignKey : PropertyStorageStyle.InlineForeignKey);
                    }  
                }
                public bool IsEmbedded { get; private set; }
            }
        } 

        public static class Validation
        {
            public class OptionalAttribute : PropertyContractAttribute
            {
                #region Overrides of PropertyContractAttribute

                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    base.ApplyTo(property, cache);
                    property.Validation.IsRequired = false;
                }

                #endregion
            }

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
            public class HiddenAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    property.DefaultDisplayVisible = false;
                }
            }
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

            public class AggregationParentAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    var relation = property.SafeGetRelation();

                    relation.Kind = RelationKind.AggregationParent;
                    relation.Multiplicity = property.IsCollection ? RelationMultiplicity.ManyToMany : RelationMultiplicity.ManyToOne;
                    relation.ThisPartyKind = RelationPartyKind.Dependent;
                }
            }

            public class CompositionParentAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    var relation = property.SafeGetRelation();

                    relation.Kind = RelationKind.CompositionParent;
                    relation.Multiplicity = RelationMultiplicity.ManyToOne;
                    relation.ThisPartyKind = RelationPartyKind.Dependent;
                }
            }

            public class CompositionAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder property, TypeMetadataCache cache)
                {
                    var relation = property.SafeGetRelation();
                    
                    relation.Kind = RelationKind.Composition;
                    relation.Multiplicity = property.IsCollection ? RelationMultiplicity.OneToMany : RelationMultiplicity.OneToOne;
                    relation.ThisPartyKind = RelationPartyKind.Principal;
                }
            }

            public class AggregationAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder metadata, TypeMetadataCache cache)
                {
                    var relation = metadata.SafeGetRelation();
                    relation.Kind = RelationKind.Aggregation;

                    if ( relation.Multiplicity != RelationMultiplicity.ManyToMany )
                    {
                        relation.Multiplicity = metadata.IsCollection ? RelationMultiplicity.OneToMany : RelationMultiplicity.OneToOne;
                    }
                    
                    relation.ThisPartyKind = RelationPartyKind.Principal;
                }
            }

            public class AssociationAttribute : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder metadata, TypeMetadataCache cache)
                {
                    var relation = metadata.SafeGetRelation();
                    relation.Kind = RelationKind.Association;
                    relation.Multiplicity = metadata.IsCollection ? RelationMultiplicity.ManyToMany : RelationMultiplicity.ManyToOne;
                    relation.ThisPartyKind = RelationPartyKind.Dependent;
                }
            }

            public class LinkToEntityAttribute : PropertyContractAttribute
            {
                public LinkToEntityAttribute(Type contractType)
                {
                    this.ContractType = contractType;
                }

                public override void ApplyTo(PropertyMetadataBuilder metadata, TypeMetadataCache cache)
                {
                    var relation = metadata.SafeGetRelation();
                    
                    if ( relation.RelatedPartyType != null )
                    {
                        return;
                    }

                    relation.ThisPartyKind = RelationPartyKind.Dependent;
                    relation.RelatedPartyType = cache.FindTypeMetadataAllowIncomplete(ContractType);
                    relation.ThisPartyKey = new KeyMetadataBuilder();
                    relation.ThisPartyKey.Name = "FK_" + relation.RelatedPartyType.Name;
                    relation.ThisPartyKey.Kind = KeyKind.Foreign;

                    if ( string.IsNullOrEmpty(this.PropertyName) )
                    {
                        if ( relation.RelatedPartyType.PrimaryKey != null )
                        {
                            relation.ThisPartyKey.Properties.AddRange(relation.RelatedPartyType.PrimaryKey.Properties);
                        }
                    }
                    else
                    {
                        var property = relation.RelatedPartyType.GetPropertyByName(this.PropertyName);
                        relation.ThisPartyKey.Properties.Add((PropertyMetadataBuilder)property);
                    }

                    if ( !metadata.DeclaringContract.AllKeys.Any(key => key.Name == relation.ThisPartyKey.Name) )
                    {
                        metadata.DeclaringContract.AllKeys.Add(relation.ThisPartyKey);
                    }
                }

                public Type ContractType { get; private set; }
                public string PropertyName { get; set; }
            }

            public class LinkToContextAttribute : PropertyContractAttribute
            {
                public LinkToContextAttribute(Type contextType)
                {
                    this.ContextType = contextType;
                }

                public override void ApplyTo(PropertyMetadataBuilder metadata, TypeMetadataCache cache)
                {
                    var relation = metadata.SafeGetRelation();
                    relation.RelatedPartyContextType = ContextType;
                }

                public Type ContextType { get; private set; }
            }

            public class NoInverseProperty : PropertyContractAttribute
            {
                public override void ApplyTo(PropertyMetadataBuilder metadata, TypeMetadataCache cache)
                {
                    metadata.SafeGetRelation().InversePropertySuppressed = true;
                }
            }
        }

        public static class Security
        {
            public class SensitiveAttribute : PropertyContractAttribute { }
        }
    }
}