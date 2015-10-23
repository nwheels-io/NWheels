using System;
using Hapil;
using NWheels.DataObjects;
using NWheels.Entities.Core;

namespace NWheels.Entities.Factories
{
    public class DomainObjectBaseTypeConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _context;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectBaseTypeConvention(DomainObjectFactoryContext context)
            : base(Will.InspectDeclaration)
        {
            _context = context;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            if ( _context.MetaType.DomainObjectType != null )
            {
                context.BaseType = _context.MetaType.DomainObjectType;
            }
            else if ( _context.MetaType.BaseType != null )
            {
                context.BaseType = ((IDomainObjectFactory)_context.BaseContext.Factory).GetOrBuildDomainObjectType(
                    _context.MetaType.BaseType.ContractType,
                    _context.PersistableFactoryType);
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetBaseType(ObjectFactoryBase factory, ITypeMetadata metaType, Type persistableObjectFactoryType)
        {
            if ( metaType.DomainObjectType != null )
            {
                return metaType.DomainObjectType;
            }
            else if ( metaType.BaseType != null )
            {
                return ((IDomainObjectFactory)factory).GetOrBuildDomainObjectType(
                    metaType.BaseType.ContractType, 
                    persistableObjectFactoryType);
            }
            else
            {
                return typeof(object);
            }
        }
    }
}

