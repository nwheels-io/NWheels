using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;

namespace NWheels.Entities.Factories
{
    public class PresentationObjectMethodsConvention : ImplementationConvention
    {
        private readonly PresentationObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PresentationObjectMethodsConvention(PresentationObjectFactoryContext context)
            : base(Will.ImplementPrimaryInterface)
        {
            _context = context;
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.AllMethods().ImplementPropagate(_context.DomainObjectField);
        }

        #endregion
    }
}
