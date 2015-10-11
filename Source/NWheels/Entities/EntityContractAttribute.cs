using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Conventions;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Extensions;

namespace NWheels.Entities
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class EntityContractAttribute : DataObjectContractAttribute
    {
        #region Overrides of DataObjectContractAttribute

        public override void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache)
        {
            base.ApplyTo(type, cache);

            type.IsAbstract = IsAbstract;
            type.IsEntity = type.ContractType.IsEntityContract();
            type.IsEntityPart = type.ContractType.IsEntityPartContract();

            if ( BaseEntity != null )
            {
                type.BaseType = cache.FindTypeMetadataAllowIncomplete(BaseEntity);
            }

            type.Name = type.Name.TrimSuffix("Entity");
            type.NamespaceQualifier = GetNamespaceQualifier(type);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAbstract { get; set; }
        public Type BaseEntity { get; set; }
        public string Namespace { get; set; }
        public bool UseCodeNamespace { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetNamespaceQualifier(TypeMetadataBuilder type)
        {
            if ( !string.IsNullOrWhiteSpace(this.Namespace) )
            {
                return this.Namespace;
            }

            if ( UseCodeNamespace && type.ContractType.Namespace != null )
            {
                var dotSeparatedParts = type.ContractType.Namespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if ( dotSeparatedParts.Length > 0 )
                {
                    return dotSeparatedParts[dotSeparatedParts.Length - 1];
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsEntityContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<EntityContractAttribute>() != null);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AggregationEntityContractAttribute : EntityContractAttribute
    {
        #region Overrides of DataObjectContractAttribute

        public override void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache)
        {
            base.ApplyTo(type, cache);
        }

        #endregion
    }
}
