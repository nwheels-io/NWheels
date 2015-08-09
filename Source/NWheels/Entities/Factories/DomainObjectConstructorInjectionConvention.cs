using System.Reflection;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects.Core.Factories;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Entities.Factories.DomainObjectFactory.TemplateTypes;

namespace NWheels.Entities.Factories
{
    public class DomainObjectConstructorInjectionConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _context;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectConstructorInjectionConvention(DomainObjectFactoryContext context)
            : base(Will.ImplementBaseClass)
        {
            _context = context;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            using ( TypeTemplate.CreateScope<TT.TContract, TT2.TDomain, TT2.TPersistable>(
                _context.MetaType.ContractType, writer.OwnerClass.TypeBuilder, _context.PersistableObjectType) )
            {
                _context.PersistableObjectField = writer.Field<TT2.TPersistable>("$persistable");

                var dependencyProperties = FindDependencyProperties(writer);

                writer.Constructor<TT.TContract, IComponentContext>((cw, persistable, components) => {
                    cw.Base();
                    _context.PersistableObjectField.Assign(persistable.CastTo<TT2.TPersistable>());

                    foreach ( var property in dependencyProperties )
                    {
                        using ( TypeTemplate.CreateScope<TT.TProperty>(property.PropertyType) )
                        {
                            cw.This<TT.TBase>()
                                .Prop<TT.TProperty>(property)
                                .Assign(Static.GenericFunc(c => Autofac.ResolutionExtensions.Resolve<TT.TProperty>(c), components));
                        }
                    }

                    PropertyImplementationStrategyMap.InvokeStrategies(
                        _context.PropertyMap.Strategies,
                        strategy => {
                            using ( _context.CreatePropertyTypeTemplateScope(strategy.MetaProperty) )
                            {
                                strategy.WriteInitialization(cw, components, persistable);
                            }
                        });
                });
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private PropertyInfo[] FindDependencyProperties(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var dependencyProperties = TypeMemberCache.Of(writer.OwnerClass.BaseType).SelectAllProperties(p => 
                p.DeclaringType != null && 
                    p.DeclaringType.IsClass &&
                    p.GetMethod != null && !p.GetMethod.IsPublic && !p.GetMethod.IsAbstract && !p.GetMethod.IsVirtual &&
                    p.SetMethod != null && !p.SetMethod.IsPublic && !p.SetMethod.IsAbstract && !p.SetMethod.IsVirtual &&
                    p.DeclaringType != typeof(object)).ToArray();

            return dependencyProperties;
        }
    }
}