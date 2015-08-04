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
    public static class TypeMetadataExtensions
    {
        public static bool TryGetImplementationBy<TFactory>(this ITypeMetadata metadata, out Type implementation)
            where TFactory : IEntityObjectFactory
        {
            return metadata.TryGetImplementation(typeof(TFactory), out implementation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool TryGetImplementationBy(this ITypeMetadata metadata, IEntityObjectFactory factory, out Type implementation)
        {
            return metadata.TryGetImplementation(factory.GetType(), out implementation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetImplementationBy(this ITypeMetadata metadata, Type factoryType)
        {
            Type implementation;

            if ( metadata.TryGetImplementation(factoryType, out implementation) )
            {
                return implementation;
            }
            
            throw new ArgumentException(string.Format(
                "Contract [{0}] has no implementation provided by factory [{1}].",
                metadata.Name, factoryType.FriendlyName()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetImplementationBy<TFactory>(this ITypeMetadata metadata) where TFactory : IEntityObjectFactory
        {
            return GetImplementationBy(metadata, typeof(TFactory));
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetImplementation(this ITypeMetadata metadata, IEntityObjectFactory factory)
        {
            return GetImplementationBy(metadata, factory.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ITypeMetadata GetRootBaseType(this ITypeMetadata metadata)
        {
            ITypeMetadata rootBaseType;

            for ( rootBaseType = metadata ; rootBaseType.BaseType != null ; rootBaseType = rootBaseType.BaseType );

            return rootBaseType;
        }
    }
}
