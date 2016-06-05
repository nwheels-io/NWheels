using System;
using System.Collections.Generic;
using Autofac;
using Hapil;
using Hapil.Applied.Conventions;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.Extensions;
using NWheels.Hosting.Core;
using NWheels.Logging.Core;
using NWheels.Logging.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Hosting.Factories
{
    public class ComponentAspectFactory : ConventionObjectFactory
    {
        private readonly IComponentContext _components;
        private readonly Pipeline<IComponentAspectProvider> _aspectPipeline;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ComponentAspectFactory(IComponentContext components, DynamicModule module, Pipeline<IComponentAspectProvider> aspectPipeline)
            : base(module)
        {
            _components = components;
            _aspectPipeline = aspectPipeline;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object Aspectize(object component, Type[] interfaceTypes = null)
        {
            var componentType = component.GetType();
            var key = new AspectWrapperTypeKey(componentType, interfaceTypes ?? componentType.GetInterfaces());
            var typeEntry = base.GetOrBuildType(key);
            return typeEntry.CreateInstance<object, object, IComponentContext>(0, component, _components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var aspectTypeKey = (AspectWrapperTypeKey)context.TypeKey;
            var staticStrings = new StaticStringsDecorator();
            var aspectContext = new ConventionContext(aspectTypeKey.TargetType, staticStrings);
            var conventions = new List<IObjectFactoryConvention>(_aspectPipeline.Count + 3);

            conventions.Add(new TargetDelegationConvention(aspectContext));
            conventions.Add(staticStrings);
            
            for (int i = _aspectPipeline.Count - 1 ; i >= 0 ; i--)
            {
                conventions.Add(_aspectPipeline[i].GetAspectConvention(aspectContext));
            }

            conventions.Add(new ConstructorInjectionConvention(aspectContext));
            return conventions.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override TypeKey CreateTypeKey(Type contractType, params Type[] secondaryInterfaceTypes)
        {
            throw new NotSupportedException();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AspectWrapperTypeKey : TypeKey
        {
            private readonly Type _targetType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AspectWrapperTypeKey(Type targetType, Type[] interfaceTypes)
                : base(
                    baseType: null,
                    primaryInterface: null,
                    secondaryInterfaces: interfaceTypes)
            {
                _targetType = targetType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type TargetType
            {
                get { return _targetType; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of TypeKey

            protected override TypeKey Mutate(Type newBaseType = null, Type newPrimaryInterface = null, Type[] newSecondaryInterfaces = null)
            {
                if (newBaseType != null || newPrimaryInterface != null)
                {
                    throw new NotSupportedException("This type key cannot mutate base type or primary interface.");
                }

                return new AspectWrapperTypeKey(
                    _targetType,
                    newSecondaryInterfaces ?? this.SecondaryInterfaces);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool Equals(TypeKey other)
            {
                var otherAspectTypeKey = (other as AspectWrapperTypeKey);

                if (otherAspectTypeKey == null)
                {
                    return false;
                }

                if (!base.Equals(other))
                {
                    return false;
                }

                return (this._targetType == otherAspectTypeKey._targetType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override int GetHashCode()
            {
                return (base.GetHashCode() ^ _targetType.GetHashCode());
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConventionContext
        {
            private readonly Dictionary<Type, FieldMember> _dependencyFieldByType;
            private Field<TT.TBase> _targetField;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ConventionContext(Type componentType, StaticStringsDecorator staticStrings)
            {
                _dependencyFieldByType = new Dictionary<Type, FieldMember>();
                this.StaticStrings = staticStrings;
                this.ComponentType = componentType;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Field<T> GetDependencyField<T>(ClassWriterBase writer)
            {
                var fieldMember = _dependencyFieldByType.GetOrAdd(typeof(T), t => writer.Field<T>("dependency$" + typeof(T).Name));
                return fieldMember.AsOperand<T>();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<FieldMember> GetAllDependencyFields()
            {
                return _dependencyFieldByType.Values;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Field<TT.TBase> GetTargetField(ClassWriterBase writer)
            {
                if (ReferenceEquals(_targetField, null))
                {
                    _targetField = writer.Field<TT.TBase>("$target");
                }

                return _targetField;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public StaticStringsDecorator StaticStrings { get; private set; }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ComponentType { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TargetDelegationConvention : ImplementationConvention
        {
            private readonly ConventionContext _aspectContext;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public TargetDelegationConvention(ConventionContext aspectContext)
                : base(Will.ImplementBaseClass | Will.ImplementAnyInterface)
            {
                _aspectContext = aspectContext;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementAnyInterface(ImplementationClassWriter<TT.TInterface> writer)
            {
                var targetField = _aspectContext.GetTargetField(writer);

                writer.AllMethods().ImplementPropagate(targetField.CastTo<TT.TInterface>());
                writer.AllProperties().ImplementPropagate(targetField.CastTo<TT.TInterface>());
                writer.AllEvents().ImplementPropagate(targetField.CastTo<TT.TInterface>());
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConstructorInjectionConvention : ImplementationConvention
        {
            private readonly ConventionContext _aspectContext;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ConstructorInjectionConvention(ConventionContext aspectContext)
                : base(Will.ImplementBaseClass)
            {
                _aspectContext = aspectContext;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
            {
                var targetField = _aspectContext.GetTargetField(writer);

                writer.Constructor<object, IComponentContext>((cw, target, components) => {
                    targetField.Assign(target.CastTo<TT.TBase>());

                    foreach (var dependencyField in _aspectContext.GetAllDependencyFields())
                    {
                        using (TT.CreateScope<TT.TDependency>(dependencyField.FieldType))
                        {
                            dependencyField.AsOperand<TT.TDependency>().Assign(
                                Static.GenericFunc(c => ResolutionExtensions.Resolve<TT.TDependency>(c), components)
                            );
                        }
                    }
                });
            }

            #endregion
        }
    }
}
