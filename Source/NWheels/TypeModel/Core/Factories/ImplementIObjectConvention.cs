using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects.Core;

namespace NWheels.TypeModel.Core.Factories
{
    public class ImplementIObjectConvention : ImplementationConvention
    {
        public ImplementIObjectConvention()
            : base(Will.ImplementBaseClass)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var objectContractType = writer.OwnerClass.Key.PrimaryInterface;

            writer.ImplementInterface<IObject>()
                .Property<Type>(x => x.ContractType).Implement(p => 
                    p.Get(w => 
                        w.Return(w.Const(objectContractType)
                    ))
                );
        }

        #endregion
    }
}
