using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using NWheels.Concurrency;
using NWheels.Concurrency.Core;
using NWheels.Extensions;
using NWheels.Processing.Commands.Impl;
using NWheels.Serialization;
using NWheels.Serialization.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Processing.Commands.Factories
{
    public class MethodCallObjectFactory : ConventionObjectFactory, IMethodCallObjectFactory
    {
        private readonly CompactSerializerFactory _serializerFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodCallObjectFactory(DynamicModule module, CompactSerializerFactory serializerFactory)
            : base(module)
        {
            _serializerFactory = serializerFactory;
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
                new BaseTypeDeferredConvention(conventionsContext), 
                new ClassNameConvention(conventionsContext),
                new ConstructorConvention(conventionsContext),
                new ParameterPropertiesConvention(conventionsContext),
                new ImplementIMethodCallObjectConvention(conventionsContext),
                new ImplementIMethodCallSerializerObjectConvention(conventionsContext)
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

                DeterminePromiseType();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MethodInfo Method { get; private set; }
            public Type TargetType { get; private set; }
            public ParameterInfo[] Parameters { get; private set; }
            public Field<TT.TProperty>[] ParameterFields { get; private set; }
            public Field<TT.TReply> ReturnValueField { get; set; }
            public Field<Dictionary<string, JToken>> ExtensionDataField { get; set; }
            public Type PromiseType { get; private set; }
            public Type PromiseTypeDefinition { get; private set; }
            public Type PromiseResultType { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool HasOutput
            {
                get
                {
                    var result = (
                        !Method.IsVoid() &&
                        (PromiseType == null || PromiseResultType != null));

                    return result;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void DeterminePromiseType()
            {
                if (Method.IsVoid())
                {
                    return;
                }

                if (Method.ReturnType == typeof(Promise) || Method.ReturnType == typeof(Task))
                {
                    PromiseType = Method.ReturnType;
                    return;
                }

                if (!Method.ReturnType.IsGenericType)
                {
                    return;
                }

                var returnTypeDefinition = Method.ReturnType.GetGenericTypeDefinition();

                if (returnTypeDefinition != typeof(Promise<>) && returnTypeDefinition != typeof(Task<>))
                {
                    return;
                }

                PromiseType = Method.ReturnType;
                PromiseTypeDefinition = returnTypeDefinition;
                PromiseResultType = PromiseType.GenericTypeArguments[0];
            }
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

        public class BaseTypeDeferredConvention : ImplementationConvention
        {
            private readonly MethodCallConventionsContext _conventionContext;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public BaseTypeDeferredConvention(MethodCallConventionsContext conventionContext)
                : base(Will.InspectDeclaration)
            {
                _conventionContext = conventionContext;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                if (_conventionContext.PromiseType == typeof(Task))
                {
                    context.BaseType = typeof(TaskBasedDeferred);
                }
                else if (_conventionContext.PromiseTypeDefinition == typeof(Task<>))
                {
                    context.BaseType = typeof(TaskBasedDeferred<>).MakeGenericType(_conventionContext.PromiseResultType);
                }
                else if (_conventionContext.PromiseResultType != null)
                {
                    context.BaseType = typeof(Deferred<>).MakeGenericType(_conventionContext.PromiseResultType);
                }
                else if (_conventionContext.PromiseType == null && !_conventionContext.Method.IsVoid())
                {
                    context.BaseType = typeof(Deferred<>).MakeGenericType(_conventionContext.Method.ReturnType);
                }
                else
                {
                    context.BaseType = typeof(Deferred);
                }
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
                                using ( TT.CreateScope<TT.TReply>(_context.Method.ReturnType) )
                                {
                                    _context.ReturnValueField = writer.Field<TT.TReply>("$returnValue");
                                    _context.ReturnValueField.Assign(target.CastTo<TT.TService>().Func<TT.TReply>(_context.Method, callArguments));
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
                        .Property(intf => intf.Serializer).Implement(p =>
                            p.Get(gw => {
                                gw.Return(gw.This<IMethodCallSerializerObject>());
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
                                    gw.Return(_context.ReturnValueField.CastTo<object>());
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ImplementIMethodCallSerializerObjectConvention : ImplementationConvention
        {
            private readonly MethodCallConventionsContext _conventionContext;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ImplementIMethodCallSerializerObjectConvention(MethodCallConventionsContext conventionContext)
                : base(Will.ImplementBaseClass)
            {
                _conventionContext = conventionContext;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                using (TT.CreateScope<TT.TReply, TT.TItem>(_conventionContext.Method.ReturnType, _conventionContext.PromiseResultType))
                {
                    var impl = writer.ImplementInterfaceExplicitly<IMethodCallSerializerObject>();

                    impl.Method<CompactSerializationContext>(x => x.SerializeInput).Implement(WriteSerializeInput);
                    impl.Method<CompactSerializationContext>(x => x.SerializeOutput).Implement(WriteSerializeOutput);
                    impl.Method<CompactDeserializationContext>(x => x.DeserializeInput).Implement(WriteDeserializeInput);
                    impl.Method<CompactDeserializationContext>(x => x.DeserializeOutput).Implement(WriteDeserializeOutput);
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void WriteSerializeInput(VoidMethodWriter writer, Argument<CompactSerializationContext> context)
            {
                foreach (var field in _conventionContext.ParameterFields)
                {
                    var parameterType = field.OperandType;
                    var valueWriter = CompactSerializerFactory.GetValueWriter(parameterType);

                    using (TT.CreateScope<TT.TProperty, TT.TItem>(parameterType, parameterType))
                    {
                        valueWriter(parameterType, writer, context, field.CastTo<TT.TItem>());
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteSerializeOutput(VoidMethodWriter writer, Argument<CompactSerializationContext> context)
            {
                if (!_conventionContext.HasOutput)
                {
                    return;
                }

                if (_conventionContext.PromiseType == null)
                {
                    WriteSerializeResultValue(writer, context, _conventionContext.Method.ReturnType, _conventionContext.ReturnValueField);
                }
                else
                {
                    using (TT.CreateScope<TT.TItem>(_conventionContext.PromiseResultType))
                    {
                        Local<TT.TItem> promiseResultLocal = writer.Local<TT.TItem>();

                        if (_conventionContext.PromiseTypeDefinition == typeof(Task<>))
                        {
                            promiseResultLocal.Assign(_conventionContext.ReturnValueField.CastTo<Task<TT.TItem>>().Prop(x => x.Result));
                        }
                        else if (_conventionContext.PromiseTypeDefinition == typeof(Promise<>))
                        {
                            promiseResultLocal.Assign(_conventionContext.ReturnValueField.CastTo<Promise<TT.TItem>>().Prop(x => x.Result));
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                        
                        WriteSerializeResultValue(
                            writer, 
                            context, 
                            _conventionContext.PromiseResultType,
                            promiseResultLocal);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteSerializeResultValue(
                VoidMethodWriter writer, 
                Argument<CompactSerializationContext> context,
                Type resultValueType,
                IOperand resultValueOperand)
            {
                var valueWriter = CompactSerializerFactory.GetValueWriter(resultValueType);

                using (TT.CreateScope<TT.TProperty, TT.TItem, TT.TReply>(resultValueType, resultValueType, resultValueType))
                {
                    valueWriter(resultValueType, writer, context, resultValueOperand.CastTo<TT.TItem>());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteDeserializeInput(VoidMethodWriter writer, Argument<CompactDeserializationContext> context)
            {
                foreach (var field in _conventionContext.ParameterFields)
                {
                    var parameterType = field.OperandType;
                    var valueReader = CompactSerializerFactory.GetValueReader(parameterType);

                    using (TT.CreateScope<TT.TProperty, TT.TItem>(parameterType, parameterType))
                    {
                        var tempLocal = writer.Local<TT.TItem>();
                        valueReader(parameterType, writer, context, tempLocal);

                        //TODO: Hapil - make ITransformType public, to avoid redundant local variable
                        field.Assign(tempLocal.CastTo<TT.TProperty>());
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteDeserializeOutput(VoidMethodWriter writer, Argument<CompactDeserializationContext> context)
            {
                if (!_conventionContext.HasOutput)
                {
                    if (_conventionContext.PromiseType == typeof(Task))
                    {
                        _conventionContext.ReturnValueField.Assign(Static.Func(Task.FromResult, writer.Const<bool>(true)).CastTo<TT.TReply>());
                    }
                    else if (_conventionContext.PromiseType == typeof(Promise))
                    {
                        _conventionContext.ReturnValueField.Assign(Static.Func(Promise.Resolved).CastTo<TT.TReply>());
                    }

                    return;
                }

                if (_conventionContext.PromiseType == null)
                {
                    var returnValueLocal = WriteDeserializeResultValue(writer, context, _conventionContext.Method.ReturnType);
                    _conventionContext.ReturnValueField.Assign(returnValueLocal.CastTo<TT.TReply>());
                }
                else
                {
                    using (TT.CreateScope<TT.TItem>(_conventionContext.PromiseResultType))
                    {
                        Local<TT.TItem> promiseResultLocal = WriteDeserializeResultValue(
                            writer,
                            context,
                            _conventionContext.PromiseResultType);

                        if (_conventionContext.PromiseTypeDefinition == typeof(Task<>))
                        {
                            _conventionContext.ReturnValueField.Assign(
                                Static.Func(
                                    Task.FromResult,
                                    promiseResultLocal).CastTo<TT.TReply>());
                        }
                        else if (_conventionContext.PromiseTypeDefinition == typeof(Promise<>))
                        {
                            _conventionContext.ReturnValueField.Assign(
                                Static.Func(
                                    Promise.Resolved,
                                    promiseResultLocal).CastTo<TT.TReply>());
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private Local<TT.TItem> WriteDeserializeResultValue(
                VoidMethodWriter writer,
                Argument<CompactDeserializationContext> context,
                Type resultValueType)
            {
                var valueReader = CompactSerializerFactory.GetValueReader(resultValueType);

                using (TT.CreateScope<TT.TProperty, TT.TItem>(resultValueType, resultValueType))
                {
                    var resultValueLocal = writer.Local<TT.TItem>();
                    valueReader(resultValueType, writer, context, resultValueLocal);

                    return resultValueLocal;

                    //TODO: Hapil - make ITransformType public, to avoid redundant local variable
                    //resultValueOperand.Assign(tempLocal.CastTo<TTValue>());
                }
            }
        }
    }
}
