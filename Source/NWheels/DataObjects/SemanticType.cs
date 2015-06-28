using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;

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

            #endregion
        }
    }
}
