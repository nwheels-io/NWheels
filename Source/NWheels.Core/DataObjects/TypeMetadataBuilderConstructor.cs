using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;

namespace NWheels.Core.DataObjects
{
    internal class TypeMetadataBuilderConstructor
    {
        public void ConstructMetadata(Type contract, TypeMetadataBuilder builder, TypeMetadataCache cache)
        {
            builder.Name = contract.Name.TrimPrefix("I");
            builder.ContractType = contract;

            foreach ( var propertyInfo in contract.GetProperties(BindingFlags.FlattenHierarchy) )
            {
                builder.Properties.Add(new PropertyMetadataBuilder() {
                    Name = propertyInfo.Name,
                    ClrType = propertyInfo.PropertyType,
                    ContractPropertyInfo = propertyInfo
                });
            }
        }
    }
}
