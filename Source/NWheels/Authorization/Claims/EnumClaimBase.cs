#if false

using System;
using System.Collections.Generic;
using System.Security.Claims;
using NWheels.Extensions;

namespace NWheels.Authorization.Claims
{
    public abstract class EnumClaimBase : Claim
    {
        protected EnumClaimBase(string type, object enumValue)
            : base(type, GetEnumValueString(enumValue), GetEnumTypeString(enumValue))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool MatchEnumValue(object enumValue)
        {
            return (this.Type == GetEnumTypeString(enumValue) && this.Value == GetEnumValueString(enumValue));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Converts an enum value to string of format EnumType.EnumValue
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumValueString(object enumValue)
        {
            if ( enumValue == null )
            {
                throw new ArgumentNullException("enumValue");
            }

            if ( !enumValue.GetType().IsEnum )
            {
                throw new ArgumentException("Value must be an enum", "enumValue");
            }

            return enumValue.GetType().Name + "." + enumValue.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumTypeString(object enumValue)
        {
            return enumValue.GetType().AssemblyQualifiedNameNonVersioned();
        }
    }
}

#endif
