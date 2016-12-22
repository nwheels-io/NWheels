using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Logging.Core
{
    public class ApplicationEventLoggerConvention : ImplementationConvention
    {
        private static readonly Type[] _s_activityNodeGenericTypeByValueCount = new Type[] {
            typeof(NameValuePairActivityLogNode),
            typeof(NameValuePairActivityLogNode<>),
            typeof(NameValuePairActivityLogNode<,>),
            typeof(NameValuePairActivityLogNode<,,>),
            typeof(NameValuePairActivityLogNode<,,,>),
            typeof(NameValuePairActivityLogNode<,,,,>),
            typeof(NameValuePairActivityLogNode<,,,,,>),
            typeof(NameValuePairActivityLogNode<,,,,,,>),
            typeof(NameValuePairActivityLogNode<,,,,,,,>)
        };

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Type[] _s_logNodeGenericTypeByValueCount = new Type[] {
            typeof(NameValuePairLogNode),
            typeof(NameValuePairLogNode<>),
            typeof(NameValuePairLogNode<,>),
            typeof(NameValuePairLogNode<,,>),
            typeof(NameValuePairLogNode<,,,>),
            typeof(NameValuePairLogNode<,,,,>),
            typeof(NameValuePairLogNode<,,,,,>),
            typeof(NameValuePairLogNode<,,,,,,>),
            typeof(NameValuePairLogNode<,,,,,,,>)
        };

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Type[] _s_genericActionTypeByValueCount = new Type[] {
            typeof(Action),
            typeof(Action<>),
            typeof(Action<,>),
            typeof(Action<,,>),
            typeof(Action<,,,>),
            typeof(Action<,,,,>),
            typeof(Action<,,,,,>),
            typeof(Action<,,,,,,>),
            typeof(Action<,,,,,,,>)
        };

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Type[] _s_genericFuncTypeByValueCount = new Type[] {
            typeof(Func<>),
            typeof(Func<,>),
            typeof(Func<,,>),
            typeof(Func<,,,>),
            typeof(Func<,,,,>),
            typeof(Func<,,,,,>),
            typeof(Func<,,,,,,>),
            typeof(Func<,,,,,,,>),
            typeof(Func<,,,,,,,,>)
        };

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly StaticStringsDecorator _staticStrings;
        private Field<IThreadLogAppender> _threadLogAppenderField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationEventLoggerConvention(StaticStringsDecorator staticStrings)
            : base(Will.ImplementBaseClass | Will.ImplementPrimaryInterface)
        {
            _staticStrings = staticStrings;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            _threadLogAppenderField = writer.Field<IThreadLogAppender>("_threadLogAppender");

            writer.Constructor<Pipeline<IThreadLogAppender>>((w, appender) => {
                _threadLogAppenderField.Assign(appender.Func<IThreadLogAppender>(x => x.AsService));
            });

            writer.AllMethods(where: IsLogMethod).Implement(ImplementLogMethod);
            
            writer.AllProperties().Implement(
                p => p.Get(w => w.Throw<NotSupportedException>("Events are not supported")),
                p => p.Set((w, value) => w.Throw<NotSupportedException>("Events are not supported")));
            
            writer.AllEvents().Implement(
                e => e.Add((w, args) => w.Throw<NotSupportedException>("Events are not supported")),  
                e => e.Remove((w, args) => w.Throw<NotSupportedException>("Events are not supported")));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsLogMethod(MethodInfo method)
        {
            if (method.DeclaringType != null && method.DeclaringType.IsInterface)
            {
                return true;
            }

            return (method.IsAbstract && method.DeclaringType != typeof(object));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementLogMethod(TemplateMethodWriter templateMethodWriter)
        {
            var logMethodWriter = new LogMethodWriter(templateMethodWriter, _threadLogAppenderField, _staticStrings);
            logMethodWriter.Write();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetMessageIdClassifier(Type loggerInterface)
        {
            if ( loggerInterface.IsNested )
            {
                if ( loggerInterface.Name.EqualsIgnoreCase("ILogger") )
                {
                    return loggerInterface.DeclaringType.Name.TrimPrefix("I");
                }
                else
                {
                    return 
                        loggerInterface.DeclaringType.Name.TrimPrefix("I") + "." + 
                        loggerInterface.Name.TrimPrefix("I").TrimSuffix("Logger");
                }
            }
            else if ( !loggerInterface.Name.EqualsIgnoreCase("ILogger") )
            {
                return loggerInterface.Name.TrimPrefix("I").TrimSuffix("Logger");
            }
            else
            {
                return loggerInterface.Namespace;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetMessageId(MethodInfo method)
        {
            return GetMessageIdClassifier(method.DeclaringType) + "." + method.Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetMessageId<TLogger>(Expression<Action<TLogger>> messageSelector)
            where TLogger : IApplicationEventLogger
        {
            var method = messageSelector.GetMethodInfo();
            return GetMessageId(method);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ContractConventionException NewContractConventionException(MemberInfo member, string message)
        {
            return new ContractConventionException(typeof(ApplicationEventLoggerConvention), TypeTemplate.Resolve<TypeTemplate.TPrimary>(), member, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LogMethodWriter
        {
            private readonly TemplateMethodWriter _underlyingWriter;
            private readonly Field<IThreadLogAppender> _threadLogAppenderField;
            private readonly StaticStringsDecorator _staticStrings;
            private readonly LogAttributeBase _attribute;
            private readonly MethodInfo _declaration;
            private readonly MethodSignature _signature;
            private readonly ParameterInfo[] _parameters;
            private readonly string _messageId;
            private readonly string _alertId;
            private readonly bool _mustCreateException;
            private readonly List<int> _exceptionArgumentIndex;
            private readonly string[] _valueArgumentFormat;
            private readonly DetailAttribute[] _valueArgumentDetails;
            private readonly bool[] _isValueArgument;
            private readonly List<IOperand> _nameValuePairLocals;
            private Type _methodCallDelegateType;
            private IOperand<Exception> _exceptionOperand;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LogMethodWriter(
                TemplateMethodWriter underlyingWriter, 
                Field<IThreadLogAppender> threadLogAppenderField, 
                StaticStringsDecorator staticStrings)
            {
                _underlyingWriter = underlyingWriter;
                _threadLogAppenderField = threadLogAppenderField;
                _staticStrings = staticStrings;
                _declaration = underlyingWriter.OwnerMethod.MethodDeclaration;
                _signature = underlyingWriter.OwnerMethod.Signature;
                _parameters = _declaration.GetParameters();
                _messageId = GetMessageId(_declaration);
                _attribute = _declaration.GetCustomAttribute<LogAttributeBase>();
                _alertId = _attribute.SystemAlertId;
                _mustCreateException = _declaration.ReturnType.IsExceptionType();
                _exceptionArgumentIndex = new List<int>();
                _isValueArgument = new bool[_signature.ArgumentCount];
                _valueArgumentFormat = new string[_signature.ArgumentCount];
                _valueArgumentDetails = new DetailAttribute[_signature.ArgumentCount];
                _nameValuePairLocals = new List<IOperand>();

                ValidateSignature();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Write()
            {
                InitializeNameValuePairs();

                if (_attribute.IsMethodCall)
                {
                    WriteMethodCallLoggedAsActivity();
                }
                else if ( _attribute.IsActivity )
                {
                    AppendAndReturnActivityLogNode();
                }
                else
                {
                    AppendLogNode();

                    if ( _mustCreateException )
                    {
                        _underlyingWriter.Return(_underlyingWriter.ReturnValueLocal);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void InitializeNameValuePairs()
            {
                for ( int i = 0 ; i < _isValueArgument.Length ; i++ )
                {
                    if ( _isValueArgument[i] )
                    {
                        InitializeNameValuePair(i);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void InitializeNameValuePair(int argumentIndex)
            {
                var m = _underlyingWriter;

                using (TT.CreateScope<TT.TValue>(_signature.ArgumentType[argumentIndex]))
                {
                    var pairLocal = m.Local<LogNameValuePair<TT.TValue>>();
                    pairLocal.Assign(m.New<LogNameValuePair<TT.TValue>>());

                    pairLocal.Field(x => x.Name).Assign(_staticStrings.GetStaticStringOperand(_signature.ArgumentName[argumentIndex]));
                    pairLocal.Field(x => x.Value).Assign(m.Argument<TT.TValue>(argumentIndex + 1));

                    if (_valueArgumentFormat[argumentIndex] != null)
                    {
                        pairLocal.Field(x => x.Format).Assign(_staticStrings.GetStaticStringOperand(_valueArgumentFormat[argumentIndex]));
                    }

                    var details = _valueArgumentDetails[argumentIndex];

                    if (details != null)
                    {
                        if (!details.IncludeInSingleLineText)
                        {
                            pairLocal.Field(x => x.IsDetail).Assign(m.Const(true));
                        }
                        if (details.Indexed)
                        {
                            pairLocal.Field(x => x.IsIndexed).Assign(m.Const(true));
                        }
                        if (details.Mutable)
                        {
                            pairLocal.Field(x => x.IsMutable).Assign(m.Const(true));
                        }
                        if (details.StatsOption != LogStatsOption.None)
                        {
                            pairLocal.Field(x => x.StatsOption).Assign(m.Const(details.StatsOption));
                        }

                        pairLocal.Field(x => x.MaxStringLength).Assign(m.Const(details.MaxStringLength));
                        pairLocal.Field(x => x.ContentTypes).Assign(m.Const(details.ContentTypes));
                    }

                    _nameValuePairLocals.Add(pairLocal);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CreateNewException()
            {
                var m = _underlyingWriter;

                var exceptionMessageLocal = m.Local<string>(
                    initialValue: _staticStrings.GetStaticStringOperand(LogMessageHelper.GetTextFromMessageId(_messageId)));
                var exceptionAnyAppendedLocal = m.Local<bool>();
                var nameValuePairIndex = 0;

                for ( int i = 0 ; i < _signature.ArgumentCount ; i++ )
                {
                    if ( !_isValueArgument[i] )
                    {
                        continue;
                    }

                    using ( TT.CreateScope<TT.TValue>(_signature.ArgumentType[i]) )
                    {
                        exceptionMessageLocal.Assign(Static.GenericFunc<string, LogNameValuePair<TT.TValue>, bool, string>(
                            (s, v, b) => LogMessageHelper.AppendToExceptionMessage<TT.TValue>(s, ref v, ref b),
                            exceptionMessageLocal,
                            _nameValuePairLocals[nameValuePairIndex].CastTo<LogNameValuePair<TT.TValue>>(),
                            exceptionAnyAppendedLocal));
                    }

                    nameValuePairIndex++;
                }

                if ( _exceptionArgumentIndex.Count > 0 )
                {
                    m.ReturnValueLocal = m.Local(initialValue: m.New<TT.TReturn>(exceptionMessageLocal, _exceptionOperand));
                }
                else
                {
                    m.ReturnValueLocal = m.Local(initialValue: m.New<TT.TReturn>(exceptionMessageLocal));
                }

                _exceptionOperand = m.ReturnValueLocal.CastTo<Exception>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendAndReturnActivityLogNode()
            {
                var activityType = ConstructGenericLogNodeType(_s_activityNodeGenericTypeByValueCount[_nameValuePairLocals.Count]);
                var m = _underlyingWriter;

                using ( TT.CreateScope<TT.TItem>(activityType) )
                {
                    var constructorArguments = 
                        new IOperand[] { _staticStrings.GetStaticStringOperand(_messageId), m.Const(_attribute.Level), m.Const(_attribute.Options) }
                        .Concat(_nameValuePairLocals)
                        .ToArray();

                    var activityLocal = m.Local<TT.TItem>(initialValue: m.New<TT.TItem>(constructorArguments));

                    if ( _attribute.IsThread )
                    {
                        _threadLogAppenderField.Void(x => x.StartThreadLog, m.Const(_attribute.TaskType), activityLocal.CastTo<ActivityLogNode>());
                    }
                    else
                    {
                        _threadLogAppenderField.Void(x => x.AppendActivityNode, activityLocal.CastTo<ActivityLogNode>());
                    }

                    m.Return(activityLocal.CastTo<TT.TReturn>());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteMethodCallLoggedAsActivity()
            {
                var activityType = ConstructGenericLogNodeType(_s_activityNodeGenericTypeByValueCount[_nameValuePairLocals.Count]);
                var m = _underlyingWriter;

                using (TT.CreateScope<TT.TItem, TT.TArg1, TT.TReturn>(activityType, _methodCallDelegateType, _declaration.ReturnType))
                {
                    var constructorArguments =
                        new IOperand[] { _staticStrings.GetStaticStringOperand(_messageId), m.Const(_attribute.Level), m.Const(_attribute.Options) }
                        .Concat(_nameValuePairLocals)
                        .ToArray();

                    var activityLocal = m.Local<TT.TItem>(initialValue: m.New<TT.TItem>(constructorArguments));

                    if (_attribute.IsThread)
                    {
                        _threadLogAppenderField.Void(x => x.StartThreadLog, m.Const(_attribute.TaskType), activityLocal.CastTo<ActivityLogNode>());
                    }
                    else
                    {
                        _threadLogAppenderField.Void(x => x.AppendActivityNode, activityLocal.CastTo<ActivityLogNode>());
                    }

                    var invokeMethodInfo = GetDelegateInvokeMethodInfo(_methodCallDelegateType);
                    var delegatedArguments = new List<IOperand>();

                    m.ForEachArgument((arg, index) => {
                        if (index > 0)
                        {
                            delegatedArguments.Add(arg);
                        }
                    });

                    m.Using(activityLocal.CastTo<IDisposable>()).Do(() => {
                        m.Try(() => {
                            if (_declaration.IsVoid())
                            {
                                m.Arg1<TT.TArg1>().Void(invokeMethodInfo, delegatedArguments.ToArray());
                            }
                            else
                            {
                                var returnValueLocal = m.Local<TT.TReturn>();
                                returnValueLocal.Assign(m.Arg1<TT.TArg1>().Func<TT.TReturn>(invokeMethodInfo, delegatedArguments.ToArray()));
                                m.Return(returnValueLocal);
                            }
                        })
                        .Catch<Exception>(e => {
                            activityLocal.CastTo<ILogActivity>().Void<Exception>(x => x.Fail, e);
                            
                            if (_attribute.ThrowOnFailure)
                            {
                                m.Throw();
                            }
                            else if (!_declaration.IsVoid())
                            {
                                m.Return(m.Default<TT.TReturn>());
                            }
                        });
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendLogNode()
            {
                var m = _underlyingWriter;

                if ( _exceptionArgumentIndex.Count > 0 )
                {
                    if ( _exceptionArgumentIndex.Count == 1 )
                    {
                        _exceptionOperand = m.Argument<Exception>(_exceptionArgumentIndex[0] + 1);
                    }
                    else
                    {
                        IOperand<Exception>[] exceptionArguments = _exceptionArgumentIndex.Select(i => m.Argument<Exception>(i + 1)).ToArray();
                        _exceptionOperand = m.New<AggregateException>(m.NewArray<Exception>(exceptionArguments));
                    }
                }
                else
                {
                    _exceptionOperand = m.Const<Exception>(null);
                }
                
                if ( _mustCreateException )
                {
                    CreateNewException();
                }

                var nodeType = ConstructGenericLogNodeType(_s_logNodeGenericTypeByValueCount[_nameValuePairLocals.Count]);

                using ( TT.CreateScope<TT.TItem>(nodeType) )
                {
                    var constructorArguments =
                        new IOperand[] { _staticStrings.GetStaticStringOperand(_messageId), m.Const(_attribute.Level), m.Const(_attribute.Options), _exceptionOperand }
                        .Concat(_nameValuePairLocals).ToArray();

                    var nodeLocal = m.Local<TT.TItem>(initialValue: m.New<TT.TItem>(constructorArguments));

                    if (!string.IsNullOrEmpty(_alertId))
                    {
                        nodeLocal.CastTo<LogNode>().Prop(x => x.AlertId).Assign(_staticStrings.GetStaticStringOperand(_alertId));
                    }

                    _threadLogAppenderField.Void(x => x.AppendLogNode, nodeLocal.CastTo<LogNode>());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Type ConstructGenericLogNodeType(Type genericDefinitionType)
            {
                if ( !genericDefinitionType.IsGenericType )
                {
                    return genericDefinitionType;
                }

                var genericArguments = new Type[_nameValuePairLocals.Count];
                var nextLocalIndex = 0;

                for ( int i = 0 ; i < _isValueArgument.Length ; i++ )
                {
                    if ( _isValueArgument[i] )
                    {
                        genericArguments[nextLocalIndex++] = _signature.ArgumentType[i];
                    }
                }

                return genericDefinitionType.MakeGenericType(genericArguments);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Type ConstructMethodCallDelegateType()
            {
                var callArgumentTypes = _parameters.Skip(1).Select(p => p.ParameterType).ToArray();

                if (callArgumentTypes.Length >= _s_genericActionTypeByValueCount.Length || callArgumentTypes.Length >= _s_genericFuncTypeByValueCount.Length)
                {
                    throw NewContractConventionException(_declaration, "Up to 8 arguments can be passed in a logged method call");
                }

                Type openGenericDelegateType;
                Type[] genericDelegateTypeArguments;

                if (_declaration.IsVoid())
                {
                    openGenericDelegateType = _s_genericActionTypeByValueCount[callArgumentTypes.Length];
                    genericDelegateTypeArguments = callArgumentTypes;
                }
                else
                {
                    openGenericDelegateType = _s_genericFuncTypeByValueCount[callArgumentTypes.Length];
                    genericDelegateTypeArguments = callArgumentTypes.ConcatOne(_declaration.ReturnType).ToArray();
                }

                if (openGenericDelegateType.IsGenericTypeDefinition)
                {
                    return openGenericDelegateType.MakeGenericType(genericDelegateTypeArguments);
                }
                else
                {
                    return openGenericDelegateType;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateSignature()
            {
                if ( _attribute == null )
                {
                    throw NewContractConventionException(_declaration, "Log attribute is missing");
                }

                if ( _attribute.IsMethodCall )
                {
                    ValidateMethodCallSignature();
                }
                else
                {
                    ValidateNonMethodCallSignature();
                }

                if ( _isValueArgument.Count(v => v) > _s_logNodeGenericTypeByValueCount.Length )
                {
                    throw NewContractConventionException(_declaration, string.Format(
                        "Method has too many values (allowed maximum is {0}, excluding exceptions).", _s_logNodeGenericTypeByValueCount.Length));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateMethodCallSignature()
            {
                if (_signature.ArgumentCount < 1)
                {
                    throw NewContractConventionException(_declaration, "First parameter must be System.Action<...> or System.Func<...> delegate");
                }

                _methodCallDelegateType = ConstructMethodCallDelegateType();

                if (_signature.ArgumentType[0] != _methodCallDelegateType)
                {
                    throw NewContractConventionException(_declaration, "Signature of logger method does not match the type of delegate");
                }

                ValidateNoRefOutParameters();

                for (int i = 1 ; i < _signature.ArgumentCount ; i++)
                {
                    _isValueArgument[i] = true;
                    _valueArgumentFormat[i] = FormatAttribute.GetFormatString(_parameters[i]);
                    _valueArgumentDetails[i] = DetailAttribute.FromParameter(_parameters[i]);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateNonMethodCallSignature()
            {
                if ( !_declaration.IsVoid() && _declaration.ReturnType != typeof(ILogActivity) && !_declaration.ReturnType.IsExceptionType() )
                {
                    throw NewContractConventionException(_declaration, "Method must either be void, return ILogActivity, or return an exception type");
                }

                ValidateNoRefOutParameters();

                for ( int i = 0 ; i < _signature.ArgumentCount ; i++ )
                {
                    if ( _signature.ArgumentType[i].IsExceptionType() )
                    {
                        _exceptionArgumentIndex.Add(i);
                    }
                    else
                    {
                        _isValueArgument[i] = true;
                        _valueArgumentFormat[i] = FormatAttribute.GetFormatString(_parameters[i]);
                        _valueArgumentDetails[i] = DetailAttribute.FromParameter(_parameters[i]);
                    }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void ValidateNoRefOutParameters()
            {
                for (int i = 0; i < _signature.ArgumentCount; i++)
                {
                    if (_signature.ArgumentIsByRef[i] || _signature.ArgumentIsOut[i])
                    {
                        throw NewContractConventionException(_declaration, "Method cannot have ref or out parameters");
                    }
                }
            }
        
            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly ConcurrentDictionary<Type, MethodInfo> _s_invokeMethodInfoByDelegateType = 
                new ConcurrentDictionary<Type, MethodInfo>();

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private static MethodInfo GetDelegateInvokeMethodInfo(Type delegateType)
            {
                return _s_invokeMethodInfoByDelegateType.GetOrAdd(
                    delegateType,
                    key => key.GetMethod("Invoke"));
            }
        }
    }
}
