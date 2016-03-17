using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.DataObjects.Core;
using NWheels.TypeModel;

namespace NWheels.DataObjects
{
    public static class SemanticType
    {
        public static readonly SemanticDataTypeAttribute Date;
        public static readonly SemanticDataTypeAttribute Time;
        public static readonly SemanticDataTypeAttribute Duration;
        public static readonly SemanticDataTypeAttribute PhoneNumber;
        public static readonly SemanticDataTypeAttribute Currency;
        public static readonly SemanticDataTypeAttribute MultilineText;
        public static readonly SemanticDataTypeAttribute EmailAddress;
        public static readonly SemanticDataTypeAttribute Password;
        public static readonly SemanticDataTypeAttribute Url;
        public static readonly SemanticDataTypeAttribute ImageUrl;
        public static readonly SemanticDataTypeAttribute CreditCard;
        public static readonly SemanticDataTypeAttribute PostalCode;
        public static readonly SemanticDataTypeAttribute Upload;
        public static readonly SemanticDataTypeAttribute Html;
        public static readonly SemanticDataTypeAttribute Xml;
        public static readonly SemanticDataTypeAttribute Json;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static SemanticType()
        {
            SemanticType.Date = new SemanticDataTypeAttribute(typeof(DefaultOf<DateTime>));
            SemanticType.Time = new SemanticDataTypeAttribute(typeof(DefaultOf<TimeSpan>));
            SemanticType.Duration = new SemanticDataTypeAttribute(typeof(DefaultOf<TimeSpan>));
            SemanticType.PhoneNumber = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.Currency = new SemanticDataTypeAttribute(typeof(DefaultOf<decimal>));
            SemanticType.MultilineText = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.EmailAddress = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.Password = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.Url = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.ImageUrl = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.CreditCard = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.PostalCode = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.Upload = new SemanticDataTypeAttribute(typeof(DefaultOf<byte[]>));
            SemanticType.Html = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.Xml = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
            SemanticType.Json = new SemanticDataTypeAttribute(typeof(DefaultOf<string>));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DefaultOf<T> : ISemanticDataType, ISemanticDataType<T>
        {
            #region ISemanticDataType<T> Members

            public bool IsValid(T value)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IPropertyValidationMetadata ISemanticDataType.GetDefaultValidation()
            {
                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            PropertyAccess? ISemanticDataType.GetDefaultPropertyAccess()
            {
                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object[] GetStandardValues()
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WellKnownSemanticType WellKnownSemantic
            {
                get
                {
                    return WellKnownSemanticType.None;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool HasStandardValues
            {
                get { return false; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool StandardValuesExclusive
            {
                get { return false; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            T ISemanticDataType<T>.DefaultValue
            {
                get
                {
                    return default(T);
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region ISemanticDataType Members

            System.ComponentModel.DataAnnotations.DataType ISemanticDataType.GetDataTypeAnnotation()
            {
                return DataType.Custom;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            string ISemanticDataType.Name
            {
                get
                {
                    return string.Format("Default<{0}>", typeof(T).FriendlyName());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type ISemanticDataType.ClrType
            {
                get
                {
                    return typeof(T);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object ISemanticDataType.DefaultValue
            {
                get
                {
                    return default(T);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            string ISemanticDataType.DefaultDisplayName
            {
                get
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            string ISemanticDataType.DefaultDisplayFormat
            {
                get
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            bool? ISemanticDataType.DefaultSortAscending
            {
                get
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TimeUnits? TimeUnits
            {
                get
                {
                    return null;
                }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class SemanticDataTypeBuilder : ISemanticDataType
        {
            protected SemanticDataTypeBuilder(string name, Type clrType)
            {
                this.Name = name;
                this.ClrType = clrType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ISemanticDataType

            public DataType GetDataTypeAnnotation()
            {
                return DataTypeAnnotation;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract IPropertyValidationMetadata GetDefaultValidation();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PropertyAccess? GetDefaultPropertyAccess()
            {
                return DefaultPropertyAccess;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Name { get; private set; }
            public Type ClrType { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WellKnownSemanticType WellKnownSemantic { get; set; }
            public object[] StandardValues { get; set; }
            public bool StandardValuesExclusive { get; set; }
            public DataType DataTypeAnnotation { get; set; }
            public PropertyAccess? DefaultPropertyAccess { get; set; }
            public string DefaultDisplayName { get; set; }
            public string DefaultDisplayFormat { get; set; }
            public bool? DefaultSortAscending { get; set; }
            public TimeUnits? TimeUnits { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract PropertyValidationMetadataBuilder DefaultValidation { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object[] ISemanticDataType.GetStandardValues()
            {
                return this.StandardValues;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            bool ISemanticDataType.HasStandardValues
            {
                get
                {
                    return (this.StandardValues != null);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object ISemanticDataType.DefaultValue
            {
                get
                {
                    return null;
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static SemanticDataTypeBuilder Create(string name, Type dataType)
            {
                return (SemanticDataTypeBuilder)Activator.CreateInstance(typeof(SemanticDataTypeBuilder<>).MakeGenericType(dataType), name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SemanticDataTypeBuilder<T> : SemanticDataTypeBuilder, ISemanticDataType, ISemanticDataType<T>
        {
            private PropertyValidationMetadataBuilder _defaultValidation = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SemanticDataTypeBuilder(string name) 
                : base(name, typeof(T))
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ISemanticDataType<T>

            public bool IsValid(T value)
            {
                if ( CustomValidator != null )
                {
                    return CustomValidator(value);
                }

                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IPropertyValidationMetadata GetDefaultValidation()
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override PropertyValidationMetadataBuilder DefaultValidation
            {
                get
                {
                    if ( _defaultValidation == null )
                    {
                        _defaultValidation = new PropertyValidationMetadataBuilder();
                    }

                    return _defaultValidation;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T DefaultValue { get; set; }
            public Func<T, bool> CustomValidator { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object ISemanticDataType.DefaultValue
            {
                get
                {
                    return DefaultValue;
                }
            }

            #endregion
        }
    }
}
