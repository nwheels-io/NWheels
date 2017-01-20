using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;

namespace NWheels.UI
{
    public class ViewModelContractAttribute : DataObjectContractAttribute
    {
        #region Overrides of DataObjectPartContractAttribute

        public override void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache)
        {
            base.ApplyTo(type, cache);

            type.IsViewModel = true;
            type.IsAbstract = this.IsAbstract;

            if (BaseViewModel != null)
            {
                type.BaseType = cache.FindTypeMetadataAllowIncomplete(BaseViewModel);
                type.BaseType.RegisterDerivedType(type);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsEntityPartContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<ViewModelContractAttribute>() != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type BaseViewModel { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAbstract { get; set; }
    }
}
