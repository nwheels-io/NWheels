using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;

namespace NWheels.Entities.Factories
{
    public class EntityContractMethodsNotSupportedConvention : ImplementationConvention
    {
        public EntityContractMethodsNotSupportedConvention()
            : base(Will.ImplementPrimaryInterface)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.AllMethods().Implement(w => w.Throw<NotSupportedException>("Methods of entity contracts are only supported by domain objects."));
        }

        #endregion
    }
}
