using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Entities.Factories.DomainObjectFactory.TemplateTypes;

namespace NWheels.Entities.Factories
{
    public class DomainObjectPropertyImplementationConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectPropertyImplementationConvention(DomainObjectFactoryContext context)
            : base(Will.ImplementBaseClass | Will.ImplementPrimaryInterface)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            //if ( _context.MetaType.DomainObjectType != null )
            //{
            //    using ( TT.CreateScope<TT.TInterface, TT2.TDomain, TT2.TPersistable>(
            //        writer.OwnerClass.BaseType, writer.OwnerClass.BaseType, _context.PersistableObjectType) )
            //    {
            //        var propertyWriter = writer.ImplementBase<TT.TInterface>();

            //        PropertyImplementationStrategyMap.InvokeStrategies(
            //            _context.PropertyMap.Strategies,
            //            strategy => {
            //                using ( _context.CreatePropertyTypeTemplateScope(strategy.MetaProperty) )
            //                {
            //                    strategy.WritePropertyImplementation(propertyWriter);
            //                }
            //            });
            //    }
            //}
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            if ( _context.MetaType.DomainObjectType == null )
            {
                //using ( TT.CreateScope<TT.TInterface, TT2.TDomain, TT2.TPersistable>(
                //    writer.OwnerClass.BaseType, writer.OwnerClass.BaseType, _context.PersistableObjectType) )
                //{
                //    PropertyImplementationStrategyMap.InvokeStrategies(
                //        _context.PropertyMap.Strategies,
                //        strategy => {
                //            using ( _context.CreatePropertyTypeTemplateScope(strategy.MetaProperty) )
                //            {
                //                strategy.WritePropertyImplementation(writer);
                //            }
                //        });
                //}

                var propertyGroupsByInterface = 
                    _context.PropertyMap.Strategies.GroupBy(strategy => strategy.MetaProperty.ContractPropertyInfo.DeclaringType);

                foreach ( var group in propertyGroupsByInterface )
                {
                    var interfaceType = group.Key;

                    using ( TT.CreateScope<TT.TInterface>(interfaceType) )
                    {
                        var explicitImplementation = writer.ImplementInterface<TT.TInterface>();

                        PropertyImplementationStrategyMap.InvokeStrategies(
                            group, 
                            strategy => {
                                using ( _context.CreatePropertyTypeTemplateScope(strategy.MetaProperty) )
                                {
                                    strategy.WritePropertyImplementation(explicitImplementation);
                                }
                            });
                    }
                }
            }
        }

        #endregion
    }
}
