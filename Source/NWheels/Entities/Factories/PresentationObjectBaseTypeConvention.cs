using System;
using Hapil;
using NWheels.DataObjects;
using NWheels.Entities.Core;

namespace NWheels.Entities.Factories
{
    public class PresentationObjectBaseTypeConvention : ImplementationConvention
    {
        private readonly PresentationObjectFactoryContext _context;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PresentationObjectBaseTypeConvention(PresentationObjectFactoryContext context)
            : base(Will.InspectDeclaration)
        {
            _context = context;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            if ( _context.MetaType.BaseType != null )
            {
                context.BaseType = GetBaseType(context, _context.MetaType);
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetBaseType(ObjectFactoryContext factoryContext, ITypeMetadata metaType)
        {
            if ( metaType.BaseType != null )
            {
                return ((IPresentationObjectFactory)factoryContext.Factory).GetOrBuildPresentationObjectType(metaType.BaseType.ContractType);
            }
            else
            {
                return typeof(object);
            }
        }
    }
}