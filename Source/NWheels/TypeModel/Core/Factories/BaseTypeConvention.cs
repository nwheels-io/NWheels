using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.DataObjects;

namespace NWheels.TypeModel.Core.Factories
{
    public class BaseTypeConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BaseTypeConvention(ITypeMetadataCache metadataCache, ITypeMetadata metaType)
            : base(Will.InspectDeclaration)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            base.Context.BaseType = GetBaseType(context.Factory, _metaType);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetBaseType(ObjectFactoryBase factory, ITypeMetadata metaType)
        {
            Type mostConcreteBaseType;

            /*if ( _metaType.DomainObjectType != null )
            {
                mostConcreteBaseType = _metaType.DomainObjectType;
            }
            else*/
            
            if ( metaType.BaseType != null )
            {
                var baseTypeKey = factory.CreateTypeKey(metaType.BaseType.ContractType);
                mostConcreteBaseType = factory.FindDynamicType(baseTypeKey);
            }
            else
            {
                mostConcreteBaseType = typeof(object);
            }

            return mostConcreteBaseType;
        }
    }
}
