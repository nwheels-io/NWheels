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
        private readonly bool _implementIsModifiedAsFalse;
        private Type _factoryType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIObjectConvention(bool implementIsModifiedAsFalse = false)
            : base(Will.InspectDeclaration | Will.ImplementBaseClass)
        {
            _implementIsModifiedAsFalse = implementIsModifiedAsFalse;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            _factoryType = context.Factory.GetType();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var objectContractType = writer.OwnerClass.Key.PrimaryInterface;
            var explicitImpl = writer.ImplementInterfaceExplicitly<IObject>();

            explicitImpl
                .Property<Type>(x => x.ContractType).Implement(p => 
                    p.Get(w => 
                        w.Return(w.Const(objectContractType))
                    )
                )
                .Property<Type>(x => x.FactoryType).Implement(p =>
                    p.Get(w =>
                        w.Return(w.Const(_factoryType))
                    )
                );

            if (_implementIsModifiedAsFalse)
            {
                explicitImpl    
                    .Property<bool>(x => x.IsModified).Implement(p => 
                        p.Get(w => 
                            w.Return(w.Const(false))
                        )
                    );

            }
        }

        #endregion
    }
}
