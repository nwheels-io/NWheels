using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;

namespace NWheels.DataObjects
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public abstract class DataObjectPartContractAttribute : Attribute, IObjectContractAttribute
    {
        public static bool IsDataObjectPartContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<DataObjectPartContractAttribute>() != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        #region Implementation of IObjectContractAttribute

        public virtual void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache)
        {
        }

        #endregion
    }
}
