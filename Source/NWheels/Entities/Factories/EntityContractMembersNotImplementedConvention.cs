using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;

namespace NWheels.Entities.Factories
{
    public class EntityContractMembersNotImplementedConvention : ImplementationConvention
    {
        public EntityContractMembersNotImplementedConvention()
            : base(Will.ImplementPrimaryInterface)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.ImplementInterfaceExplicitly<TypeTemplate.TInterface>()
                .AllProperties().Implement(
                    p => p.Get(w => w.Throw<NotImplementedException>("Not implemented")),
                    p => p.Set((w, value) => w.Throw<NotImplementedException>("Not implemented"))
                 )
                .AllMethods().Throw<NotImplementedException>();
        }

        #endregion
    }
}
