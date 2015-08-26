using Hapil;
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
                context.BaseType = 
                    ((IPresentationObjectFactory)_context.BaseContext.Factory).GetOrBuildPresentationObjectType(_context.MetaType.BaseType.ContractType);
            }
        }

        #endregion
    }
}