using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public object CreateProxy(object component, Type[] interfaceTypes = null)
        {
            var componentType = component.GetType();
            var key = new AspectTypeKey(
                ImplementationMode.Wrapper,
                targetType: componentType, 
                baseType: null, 
                interfaceTypes: interfaceTypes ?? componentType.GetInterfaces());
            
            var typeEntry = base.GetOrBuildType(key);
            return typeEntry.CreateInstance<object, object, IComponentContext>(0, component, _components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInheritor(Type componentType)
        {
            var key = new AspectTypeKey(
                ImplementationMode.Inheritor,
                targetType: null,
                baseType: componentType,
                interfaceTypes: componentType.GetInterfaces());

            var typeEntry = base.GetOrBuildType(key);
            return typeEntry.CreateInstance<object, IComponentContext>(0, _components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var aspectTypeKey = (AspectTypeKey)context.TypeKey;
            var staticStrings = new StaticStringsDecorator();
            var aspectContext = new ConventionContext(
                staticStrings, 
                aspectTypeKey.Mode,
                componentType: aspectTypeKey.TargetType ?? aspectTypeKey.BaseType);
            var conventions = new List<IObjectFactoryConvention>(_aspectPipeline.Count + 3);

            switch (aspectContext.ImplementationMode)
            {
                case ImplementationMode.Wrapper:
                    conventions.Add(new TargetDelegationConvention(aspectContext));
                    conventions.Add(staticStrings);
                    AddAspectConventions(conventions, aspectContext);
                    conventions.Add(new WrapperConstructorInjectionConvention(aspectContext));
                    break;
                case ImplementationMode.Inheritor:
                    conventions.Add(new BaseDelegationConvention(aspectContext));
                    conventions.Add(staticStrings);
                    AddAspectConventions(conventions, aspectContext);
                    conventions.Add(new BaseConstructorInjectionConvention(aspectContext));
                    break;
            }

            return conventions.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override TypeKey CreateTypeKey(Type contractType, params Type[] secondaryInterfaceTypes)
        {
            throw new NotSupportedException();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddAspectConventions(List<IObjectFactoryConvention> destination, ConventionContext aspectContext)
        {
            for (int i = _aspectPipeline.Count - 1; i >= 0; i--)
            {
                destination.Add(_aspectPipeline[i].GetAspectConvention(aspectContext));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void WriteResolveDependencies(ConventionContext context, Argument<IComponentContext> components)
        {
            foreach (var dependencyField in context.GetAllDependencyFields())
            {
                using (TT.CreateScope<TT.TDependency>(dependencyField.FieldType))
                {
                    dependencyField.AsOperand<TT.TDependency>().Assign(Static.GenericFunc(c => ResolutionExtensions.Resolve<TT.TDependency>(c), components));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum ImplementationMode
        {
            Wrapper,
            Inheritor
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AspectTypeKey : TypeKey
        {
            private readonly ImplementationMode _mode;
            private readonly Type _targetType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AspectTypeKey(ImplementationMode mode, Type targetType, Type baseType, Type[] interfaceTypes)
                : base(
                    baseType,
                    primaryInterface: null,
                    secondaryInterfaces: interfaceTypes)
            {
                _mode = mode;
                _targetType = targetType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type TargetType
            {
                get { return _targetType; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ImplementationMode Mode
            {
                get { return _mode; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of TypeKey

            protected override TypeKey Mutate(Type newBaseType = null, Type newPrimaryInterface = null, Type[] newSecondaryInterfaces = null)
            {
                if (newBaseType != null || newPrimaryInterface != null)
                {
                    throw new NotSupportedException("This type key cannot mutate base type or primary interface.");
                }

                return new AspectTypeKey(
                    _mode,
                    _targetType,
                    this.BaseType,
                    newSecondaryInterfaces ?? this.SecondaryInterfaces);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool Equals(TypeKey other)
            {
                var otherAspectTypeKey = (other as AspectTypeKey);

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
                if (_targetType != null)
                {
                    return (base.GetHashCode() ^ _targetType.GetHashCode());
                }

                return base.GetHashCode();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConventionContext
        {
            private readonly Dictionary<Type, FieldMember> _dependencyFieldByType;
            private Field<TT.TBase> _targetField;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ConventionContext(StaticStringsDecorator staticStrings, ImplementationMode implementationMode, Type componentType)
            {
                _dependencyFieldByType = new Dictionary<Type, FieldMember>();
                this.StaticStrings = staticStrings;
                this.ImplementationMode = implementationMode;
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

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ImplementationMode ImplementationMode { get; private set; }
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

        public class BaseDelegationConvention : ImplementationConvention
        {
            private readonly ConventionContext _aspectContext;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public BaseDelegationConvention(ConventionContext aspectContext)
                : base(Will.ImplementBaseClass)
            {
                _aspectContext = aspectContext;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                writer.AllMethods().Implement(w => 
                    w.ProceedToBase()
                );
                writer.ReadOnlyProperties().Implement(
                    p => p.Get(gw => gw.ProceedToBase())
                );
                writer.ReadWriteProperties().Implement(
                    p => p.Get(gw => gw.ProceedToBase()),
                    p => p.Set((sw, value) => sw.ProceedToBase())
                );
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WrapperConstructorInjectionConvention : ImplementationConvention
        {
            private readonly ConventionContext _aspectContext;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public WrapperConstructorInjectionConvention(ConventionContext aspectContext)
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
                    WriteResolveDependencies(_aspectContext, components);
                });
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BaseConstructorInjectionConvention : ImplementationConvention
        {
            private readonly ConventionContext _aspectContext;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public BaseConstructorInjectionConvention(ConventionContext aspectContext)
                : base(Will.ImplementBaseClass)
            {
                _aspectContext = aspectContext;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
            {
                var baseConstructor = _aspectContext.ComponentType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .OrderByDescending(c => c.GetParameters().Length)
                    .First();
                var baseConstructorParameters = baseConstructor.GetParameters();

                writer.Constructor<IComponentContext>((cw, components) =>
                {
                    var baseConstructorArgumentLocals = new Local<TT.TArgument>[baseConstructorParameters.Length];

                    for (int i = 0; i < baseConstructorParameters.Length; i++)
                    {
                        using (TT.CreateScope<TT.TArgument>(baseConstructorParameters[i].ParameterType))
                        {
                            baseConstructorArgumentLocals[i] = cw.Local<TT.TArgument>();
                            baseConstructorArgumentLocals[i].Assign(Static.GenericFunc(c => ResolutionExtensions.Resolve<TT.TArgument>(c), components));
                        }
                    }

                    cw.Base(baseConstructor, baseConstructorArgumentLocals.Cast<IOperand>().ToArray());
                    
                    WriteResolveDependencies(_aspectContext, components);
                });
            }

            #endregion
        }
    }
}
