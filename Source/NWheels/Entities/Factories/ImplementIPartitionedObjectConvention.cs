using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class ImplementIPartitionedObjectConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIPartitionedObjectConvention(ITypeMetadata metaType)
            : base(Will.ImplementBaseClass)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_metaType.PartitionProperty != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            using ( TT.CreateScope<TT.TContract, TT.TValue>(_metaType.ContractType, _metaType.PartitionProperty.ClrType) )
            {
                writer.ImplementInterfaceExplicitly<IPartitionedObject>()
                    .Property(x => x.PartitionValue).Implement(p => 
                        p.Get(w => 
                            w.Return(w.This<TT.TContract>().Prop<TT.TValue>(_metaType.PartitionProperty.ContractPropertyInfo)
                        )
                    )
                );
            }
        }

        #endregion
    }
}
