using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;

namespace NWheels.Entities.Factories
{
    public class DomainObjectMethodsConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectMethodsConvention(DomainObjectFactoryContext context)
            : base(Will.ImplementPrimaryInterface)
        {
            _context = context;
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_context.MetaType.DomainObjectType == null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.AllMethods().Implement(w => w.Throw<NotSupportedException>("Entity methods are not supported by automatic domain objects."));
        }

        #endregion
    }
}
