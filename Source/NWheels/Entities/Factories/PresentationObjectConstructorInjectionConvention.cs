using System.Reflection;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Core;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Entities.Factories.DomainObjectFactory.TemplateTypes;

namespace NWheels.Entities.Factories
{
    public class PresentationObjectConstructorInjectionConvention : ImplementationConvention
    {
        private readonly PresentationObjectFactoryContext _context;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PresentationObjectConstructorInjectionConvention(PresentationObjectFactoryContext context)
            : base(Will.ImplementPrimaryInterface)
        {
            _context = context;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            _context.DomainObjectField = writer.Field<TT.TInterface>("$domainObject");
            _context.PresentationFactoryField = writer.Field<IPresentationObjectFactory>("$presentationFactory");

            writer.Constructor<TT.TInterface, IComponentContext>((cw, domainObject, components) => {
                if ( _context.MetaType.BaseType != null )
                {
                    cw.Base(domainObject, components);
                }
                else
                {
                    cw.Base();
                }

                _context.DomainObjectField.Assign(domainObject);
                _context.PresentationFactoryField.Assign(Static.GenericFunc(c => ResolutionExtensions.Resolve<IPresentationObjectFactory>(c), components));

                PropertyImplementationStrategyMap.InvokeStrategies(
                    _context.PropertyMap.Strategies,
                    strategy => {
                        strategy.WriteInitialization(cw, components, domainObject);
                    });
            });
        }

        #endregion
    }
}