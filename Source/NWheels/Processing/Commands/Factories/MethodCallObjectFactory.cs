using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Toolbox;
using Hapil.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NWheels.Extensions;
using NWheels.Processing.Commands.Impl;
using TT = Hapil.TypeTemplate;

namespace NWheels.Processing.Commands.Factories
{
    public class MethodCallObjectFactory : ConventionObjectFactory, IMethodCallObjectFactory
    {
        public MethodCallObjectFactory(DynamicModule module)
            : base(module)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMethodCallObjectFactory

        public Type GetMessageCallObjectType(MethodInfo method)
        {
            var typeEntry = GetOrBuildType(new MethodCallTypeKey(method));
            return typeEntry.DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IMethodCallObject NewMessageCallObject(MethodInfo method)
        {
            var typeEntry = GetOrBuildType(new MethodCallTypeKey(method));
            return typeEntry.CreateInstance<IMethodCallObject>(factoryMethodIndex: 0);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var conventionsContext = new MethodCallConventionsContext(((MethodCallTypeKey)context.TypeKey).Method);

            return new IObjectFactoryConvention[] {
                new ClassNameConvention(conventionsContext),
                new ConstructorConvention(conventionsContext),
                new ParameterPropertiesConvention(conventionsContext),
                new ImplementIMethodCallObjectConvention(conventionsContext)
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MethodCallConventionsContext
        {
            public MethodCallConventionsContext(MethodInfo method)
            {
                this.Method = method;
                this.TargetType = method.DeclaringType;
                this.Parameters = method.GetParameters();
                this.ParameterFields = new Field<TypeTemplate.TProperty>[this.Parameters.Length];
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MethodInfo Method { get; private set; }
            public Type TargetType { get; private set; }
            public ParameterInfo[] Parameters { get; private set; }
            public Field<TT.TProperty>[] ParameterFields { get; private set; }
            public Field<TT.TReturn> ReturnValueField { get; set; }
            public Field<Dictionary<string, JToken>> ExtensionDataField { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MethodCallTypeKey : TypeKey
        {
            public MethodCallTypeKey(MethodInfo method, Type baseType = null, Type primaryInterface = null, Type[] secondaryInterfaces = null)
                : base(baseType, primaryInterface, secondaryInterfaces)
            {
                this.Method = method;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of TypeKey

            public override bool Equals(TypeKey other)
            {
                var otherMethodCallTypeKey = other as MethodCallTypeKey;

                if ( otherMethodCallTypeKey != null )
                {
                    return this.Method == otherMethodCallTypeKey.Method;
                }
                else
                {
                    return false;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override TypeKey Mutate(Type newBaseType = null, Type newPrimaryInterface = null, Type[] newSecondaryInterfaces = null)
            {
                return new MethodCallTypeKey(
                    this.Method, 
                    newBaseType ?? this.BaseType, 
                    newPrimaryInterface ?? this.PrimaryInterface, 
                    newSecondaryInterfaces ?? this.SecondaryInterfaces);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MethodInfo Method { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ClassNameConvention : ImplementationConvention
        {
            private readonly MethodCallConventionsContext _conventionContext;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ClassNameConvention(MethodCallConventionsContext conventionContext)
                : base(Will.InspectDeclaration)
            {
                _conventionContext = conventionContext;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                var currentNameParts = context.ClassFullName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                context.ClassFullName = string.Format(
                    "{0}MethodCall_{1}_{2}",
                    (currentNameParts.Length > 0 ? string.Join(".", currentNameParts.Take(currentNameParts.Length - 1)) + "." : ""),
                    _conventionContext.Method.DeclaringType.SimpleQualifiedName().Replace(".", "_"),
                    _conventionContext.Method.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConstructorConvention : ImplementationConvention
        {
            private readonly MethodCallConventionsContext _context;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ConstructorConvention(MethodCallConventionsContext context)
                : base(Will.ImplementBaseClass)
            {
                _context = context;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                _context.ExtensionDataField = writer.Field<Dictionary<string, JToken>>("$extensionData");

                writer.DefaultConstructor(w => {
                    _context.ExtensionDataField.Assign(w.New<Dictionary<string, JToken>>());
                });
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ParameterPropertiesConvention : ImplementationConvention
        {
            private readonly MethodCallConventionsContext _context;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ParameterPropertiesConvention(MethodCallConventionsContext context)
                : base(Will.ImplementBaseClass)
            {
                _context = context;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                for ( int i = 0 ; i < _context.Parameters.Length ; i++ )
                {
                    using ( TT.CreateScope<TT.TProperty>(_context.Parameters[i].ParameterType) )
                    {
                        var parameterNameInPascalCase = _context.Parameters[i].Name.ToPascalCase();

                        _context.ParameterFields[i] = writer.Field<TT.TProperty>("m_" + parameterNameInPascalCase);
                        writer.NewVirtualWritableProperty<TT.TProperty>(parameterNameInPascalCase).ImplementAutomatic(_context.ParameterFields[i]);
                    }
                }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ImplementIMethodCallObjectConvention : ImplementationConvention
        {
            private readonly MethodCallConventionsContext _context;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ImplementIMethodCallObjectConvention(MethodCallConventionsContext context)
                : base(Will.ImplementBaseClass)
            {
                _context = context;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                var callArguments = _context.ParameterFields.Cast<IOperand>().ToArray();

                using ( TT.CreateScope<TT.TService>(_context.TargetType) )
                {
                    writer.ImplementInterfaceExplicitly<IMethodCallObject>()
                        .Method<object>(intf => intf.ExecuteOn).Implement((w, target) => {
                            if ( _context.Method.IsVoid() )
                            {
                                target.CastTo<TT.TService>().Void(_context.Method, callArguments);
                            }
                            else
                            {
                                using ( TT.CreateScope<TT.TReturn>(_context.Method.ReturnType) )
                                {
                                    _context.ReturnValueField = writer.Field<TT.TReturn>("$returnValue");
                                    _context.ReturnValueField.Assign(target.CastTo<TT.TService>().Func<TT.TReturn>(_context.Method, callArguments));
                                }
                            }
                        })
                        .Method<int, object>(intf => intf.GetParameterValue).Implement((w, index) => {
                            if ( callArguments.Length > 0 )
                            {
                                var switchStatement = w.Switch(index);
                                for ( int i = 0; i < callArguments.Length; i++ )
                                {
                                    var argumentIndex = i;
                                    switchStatement.Case(i).Do(() => {
                                        w.Return(callArguments[argumentIndex].CastTo<object>());
                                    });
                                }
                            }
                            w.Throw<ArgumentOutOfRangeException>("Argument index was out of range.");
                        })
                        .Method<string, object>(intf => intf.GetParameterValue).Implement((w, name) => {
                            for (int i = 0; i < callArguments.Length; i++)
                            {
                                var argumentIndex = i;
                                w.If(Static.Func(NWheels.Extensions.StringExtensions.EqualsIgnoreCase, name, w.Const(_context.Parameters[argumentIndex].Name))).Then(() => {
                                    w.Return(callArguments[argumentIndex].CastTo<object>());
                                });
                            }
                            w.Throw<ArgumentOutOfRangeException>("Argument index was out of range.");
                        })
                        .Method<int, object>(intf => intf.SetParameterValue).Implement((w, index, value) => {
                            if (callArguments.Length > 0)
                            {
                                var switchStatement = w.Switch(index);
                                for (int i = 0; i < callArguments.Length; i++)
                                {
                                    using (TT.CreateScope<TT.TProperty>(_context.Parameters[i].ParameterType))
                                    {
                                        var argumentIndex = i;
                                        switchStatement.Case(i).Do(() => {
                                            ((Field<TT.TProperty>)callArguments[argumentIndex]).Assign(value.CastTo<TT.TProperty>());
                                            w.Return();
                                        });
                                    }
                                }
                            }
                            w.Throw<ArgumentOutOfRangeException>("Argument index was out of range.");
                        })
                        .Method<string, object>(intf => intf.SetParameterValue).Implement((w, name, value) => {
                            for (int i = 0; i < callArguments.Length; i++)
                            {
                                using (TT.CreateScope<TT.TProperty>(_context.Parameters[i].ParameterType))
                                {
                                    var argumentIndex = i;
                                    w.If(Static.Func(NWheels.Extensions.StringExtensions.EqualsIgnoreCase, name, w.Const(_context.Parameters[argumentIndex].Name))).Then(() => {
                                        ((Field<TT.TProperty>)callArguments[argumentIndex]).Assign(value.CastTo<TT.TProperty>());
                                        w.Return();
                                    });
                                }
                            }
                            w.Throw<ArgumentOutOfRangeException>("Argument index was out of range.");
                        })
                        .Property(intf => intf.MethodInfo).Implement(p =>
                            p.Get(gw => {
                                gw.Return(gw.Const<MethodInfo>(_context.Method));                        
                            })
                        )
                        .Property(intf => intf.Result).Implement(p => 
                            p.Get(gw => {
                                if ( _context.Method.IsVoid() )
                                {
                                    gw.Return(gw.Const<object>(null));
                                }
                                else
                                {
                                    gw.Return(_context.ReturnValueField);
                                }
                            })
                        )
                        .Property(intf => intf.CorrelationId).ImplementAutomatic()
                        .Property(intf => intf.ExtensionData).Implement(
                            Attributes.Set<JsonExtensionDataAttribute>(),
                            p => p.Get(gw => {
                                gw.Return(_context.ExtensionDataField);
                            })
                        );
                }
            }

            #endregion
        }
    }
}
