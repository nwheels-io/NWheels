using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Conventions.Core;
using NWheels.DataObjects;

namespace NWheels.Extensions
{
    public static class PropertyMetadataExtensions
    {
        public static bool TryGetImplementationBy<TFactory>(this IPropertyMetadata metadata, out PropertyInfo implementation)
            where TFactory : IEntityObjectFactory
        {
            return metadata.TryGetImplementation(typeof(TFactory), out implementation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PropertyInfo GetImplementationBy(this IPropertyMetadata metadata, Type factoryType)
        {
            PropertyInfo implementation;

            if ( metadata.TryGetImplementation(factoryType, out implementation) )
            {
                return implementation;
            }
            
            throw new ArgumentException(string.Format(
                "Property [{0}] has no implementation provided by factory [{1}].",
                metadata.ContractQualifiedName, factoryType.FriendlyName()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PropertyInfo GetImplementationBy<TFactory>(this IPropertyMetadata metadata) where TFactory : IEntityObjectFactory
        {
            return GetImplementationBy(metadata, typeof(TFactory));
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static PropertyInfo GetImplementationBy(this IPropertyMetadata metadata, IEntityObjectFactory factory)
        {
            return GetImplementationBy(metadata, factory.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type StorageClrType(this IPropertyMetadata metadata)
        {
            return (metadata.RelationalMapping.StorageType != null
                ? metadata.RelationalMapping.StorageType.StorageDataType
                : metadata.ClrType);
        }
    }
}
