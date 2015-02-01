using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Logging;
using TT = Hapil.TypeTemplate;

namespace NWheels.Core.Logging
{
    public class ApplicationEventLoggerConvention : ImplementationConvention
    {
        private Field<IThreadLogAppender> _threadLogAppenderField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationEventLoggerConvention()
            : base(Will.ImplementPrimaryInterface)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            _threadLogAppenderField = writer.Field<IThreadLogAppender>("_threadLogAppender");

            writer.Constructor<IThreadLogAppender>((w, appender) => _threadLogAppenderField.Assign(appender));
            
            writer.AllMethods().Implement(ImplementLogMethod);
            
            writer.AllProperties().Implement(
                p => p.Get(w => w.Throw<NotSupportedException>("Events are not supported")),
                p => p.Set((w, value) => w.Throw<NotSupportedException>("Events are not supported")));
            
            writer.AllEvents().Implement(
                e => e.Add((w, args) => w.Throw<NotSupportedException>("Events are not supported")),  
                e => e.Remove((w, args) => w.Throw<NotSupportedException>("Events are not supported")));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementLogMethod(TemplateMethodWriter templateMethodWriter)
        {
            var logMethodWriter = new LogMethodWriter(templateMethodWriter, _threadLogAppenderField);
            logMethodWriter.Write();
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
            private readonly LogAttributeBase _attribute;
            private readonly MethodInfo _declaration;
            private readonly MethodSignature _signature;
            private readonly ParameterInfo[] _parameters;
            private readonly bool _mustCreateException;
            private readonly List<int> _exceptionArgumentIndex;
            private readonly List<int> _formatArgumentIndex;
            private readonly List<int> _detailArgumentIndex;
            private string _formatString = null;
            private Local<object[]> _formatObjectArray = null;
            private IOperand<string> _singleLineTextLocal;
            private Local<StringBuilder> _fullDetailsTextLocal;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LogMethodWriter(TemplateMethodWriter underlyingWriter, Field<IThreadLogAppender> threadLogAppenderField)
            {
                _underlyingWriter = underlyingWriter;
                _threadLogAppenderField = threadLogAppenderField;
                _declaration = underlyingWriter.OwnerMethod.MethodDeclaration;
                _signature = underlyingWriter.OwnerMethod.Signature;
                _parameters = _declaration.GetParameters();
                _attribute = _declaration.GetCustomAttribute<LogAttributeBase>();
                _mustCreateException = _declaration.ReturnType.IsExceptionType();
                _exceptionArgumentIndex = new List<int>();
                _formatArgumentIndex = new List<int>();
                _detailArgumentIndex = new List<int>();

                ValidateSignature();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Write()
            {
                BuildSingleLineText();
                BuildFullDetailsText();

                if ( _attribute.IsActivity )
                {
                    AppendAndReturnActivityLogNode();
                }
                else
                {
                    if ( _mustCreateException )
                    {
                        CreateNewException();
                    }

                    AppendLogNode();

                    if ( _mustCreateException )
                    {
                        _underlyingWriter.Return(_underlyingWriter.ReturnValueLocal);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void BuildSingleLineText()
            {
                if ( _formatArgumentIndex.Count > 0 )
                {
                    BuildFormatString();
                    CopyArgumentsToArray();
                    FormatLogText();
                }
                else
                {
                    _singleLineTextLocal = _underlyingWriter.Const(_declaration.Name.SplitPascalCase());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CopyArgumentsToArray()
            {
                var m = _underlyingWriter;

                _formatObjectArray = m.Local(initialValue: m.NewArray<object>(length: m.Const(_formatArgumentIndex.Count)));

                for ( int i = 0 ; i < _formatArgumentIndex.Count ; i++ )
                {
                    var argumentIndex = _formatArgumentIndex[i];

                    using ( TT.CreateScope<TT.TArgument>(_signature.ArgumentType[argumentIndex]) )
                    {
                        _formatObjectArray.ItemAt(i).Assign(m.Argument<TT.TArgument>(argumentIndex + 1).CastTo<object>());
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FormatLogText()
            {
                var m = _underlyingWriter;
                _singleLineTextLocal = m.Local(initialValue: Static.Func(string.Format, m.Const(_formatString), _formatObjectArray));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void BuildFullDetailsText()
            {
                for ( int i = 0 ; i < _detailArgumentIndex.Count ; i++ )
                {
                    AppendParameterToFullDetails(_parameters[_detailArgumentIndex[i]]);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void EnsureFullDetailsTextInitialized()
            {
                if ( object.ReferenceEquals(_fullDetailsTextLocal, null) )
                {
                    var m = _underlyingWriter;
                    _fullDetailsTextLocal = m.Local(initialValue: m.New<StringBuilder>());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendParameterToFullDetails(ParameterInfo parameter)
            {
                EnsureFullDetailsTextInitialized();

                var m = _underlyingWriter;
                var formatSpec = FormatAttribute.GetFormatString(parameter);
                var format = parameter.Name.ToPascalCase() + "=" + (formatSpec == null ? "{0}" : "{0:" + formatSpec + "}");

                using ( TT.CreateScope<TT.TArgument>(parameter.ParameterType) )
                {
                    _fullDetailsTextLocal.Func<string, object, StringBuilder>(
                        x => x.AppendFormat, 
                        m.Const(format), 
                        m.Argument<TT.TArgument>(parameter.Position + 1).CastTo<object>());

                    _fullDetailsTextLocal.Func<StringBuilder>(x => x.AppendLine);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendExceptionToFullDetails(IOperand<Exception> exceptionOperand)
            {
                EnsureFullDetailsTextInitialized();
                _fullDetailsTextLocal.Func<string, StringBuilder>(x => x.AppendLine, exceptionOperand.FuncToString());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CreateNewException()
            {
                var m = _underlyingWriter;
                m.ReturnValueLocal = m.Local(initialValue: m.New<TT.TReturn>(_singleLineTextLocal));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendAndReturnActivityLogNode()
            {
                var m = _underlyingWriter;

                var activityLocal = m.Local<ActivityLogNode>(initialValue: m.New<FormattedActivityLogNode>(_singleLineTextLocal));

                if ( _attribute.IsThread )
                {
                    _threadLogAppenderField.Void(x => x.StartThreadLog, m.Const(_attribute.TaskType), activityLocal);
                }
                else
                {
                    _threadLogAppenderField.Void(x => x.AppendActivityNode, activityLocal);
                }

                m.Return(activityLocal.CastTo<TT.TReturn>());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendLogNode()
            {
                var m = _underlyingWriter;
                IOperand<Exception> exceptionOperand;

                if ( _exceptionArgumentIndex.Count > 0  )
                {
                    if ( _exceptionArgumentIndex.Count == 1 )
                    {
                        exceptionOperand = m.Argument<Exception>(_exceptionArgumentIndex[0] + 1);
                    }
                    else
                    {
                        IOperand<Exception>[] exceptionArguments = _exceptionArgumentIndex.Select(i => m.Argument<Exception>(i + 1)).ToArray();
                        exceptionOperand = m.New<AggregateException>(m.NewArray<Exception>(exceptionArguments));
                    }

                    AppendExceptionToFullDetails(exceptionOperand);
                }
                else if ( _mustCreateException )
                {
                    exceptionOperand = m.ReturnValueLocal.CastTo<Exception>();
                    AppendExceptionToFullDetails(exceptionOperand);
                }
                else
                {
                    exceptionOperand = m.Const<Exception>(null);
                }

                IOperand<string> fullDetailsTextOperand = (
                    object.ReferenceEquals(_fullDetailsTextLocal, null) ? 
                    m.Const<string>(null) :
                    _fullDetailsTextLocal.FuncToString());

                _threadLogAppenderField.Void(x => x.AppendLogNode, m.New<FormattedLogNode>(
                    m.Const(_attribute.Level),
                    _singleLineTextLocal,
                    fullDetailsTextOperand,
                    m.Const(LogContentTypes.Text),
                    exceptionOperand));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateSignature()
            {
                if ( _attribute == null )
                {
                    throw NewContractConventionException(_declaration, "Log attribute is missing");
                }

                if ( !_declaration.IsVoid() && _declaration.ReturnType != typeof(ILogActivity) && !_declaration.ReturnType.IsExceptionType() )
                {
                    throw NewContractConventionException(_declaration, "Method must either be void, return ILogActivity, or return an exception type");
                }

                for ( int i = 0 ; i < _signature.ArgumentCount ; i++ )
                {
                    if ( _signature.ArgumentIsByRef[i] || _signature.ArgumentIsOut[i] )
                    {
                        throw NewContractConventionException(_declaration, "Method cannot have ref or out parameters");
                    }

                    if ( _signature.ArgumentType[i].IsExceptionType() )
                    {
                        _exceptionArgumentIndex.Add(i);
                    }
                    else if ( DetailAttribute.IsDefinedOn(_parameters[i]) )
                    {
                        _detailArgumentIndex.Add(i);
                    }
                    else
                    {
                        _formatArgumentIndex.Add(i);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void BuildFormatString()
            {
                var formatStringBuilder = new StringBuilder(_declaration.Name.SplitPascalCase());

                for ( int i = 0 ; i < _formatArgumentIndex.Count ; i++ )
                {
                    var formatString = FormatAttribute.GetFormatString(_parameters[_formatArgumentIndex[i]]);

                    formatStringBuilder.Append(i > 0 ? ", " : ": ");
                    formatStringBuilder.AppendFormat(
                        "{0}={{{1}{2}}}", 
                        _signature.ArgumentName[_formatArgumentIndex[i]], 
                        i,
                        formatString != null ? ":" + formatString : "");
                }

                _formatString = formatStringBuilder.ToString();
            }
        }
    }
}
