#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class EnsureDomainObjectConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EnsureDomainObjectConvention(ITypeMetadata metaType)
            : base(Will.ImplementBaseClass)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var componentsField = writer.DependencyField<IComponentContext>("$components");

            writer.ImplementInterfaceExplicitly<IEntityPartObject>()
                .Method(x => x.EnsureDomainObject).Implement(m => {
                    using (TT.CreateScope<TT.TContract>(_metaType.ContractType))
                    {
                        Static.GenericFunc((o, c) => RuntimeEntityModelHelpers.EnsureContainerDomainObject<TT.TContract>(o, c),
                            m.This<IPersistableObject>(),
                            componentsField);
                    }
                });
        }
    }
}

#endif