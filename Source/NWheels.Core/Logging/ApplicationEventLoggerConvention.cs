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
            private readonly bool _mustCreateException;
            private readonly List<int> _exceptionArgumentIndex;
            private readonly List<int> _formatArgumentIndex;
            private string _formatString = null;
            private Local<object[]> _formatObjectArray = null;
            private IOperand<string> _singleLineTextLocal;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LogMethodWriter(TemplateMethodWriter underlyingWriter, Field<IThreadLogAppender> threadLogAppenderField)
            {
                _underlyingWriter = underlyingWriter;
                _threadLogAppenderField = threadLogAppenderField;
                _declaration = underlyingWriter.OwnerMethod.MethodDeclaration;
                _signature = underlyingWriter.OwnerMethod.Signature;
                _attribute = _declaration.GetCustomAttribute<LogAttributeBase>();
                _mustCreateException = _declaration.ReturnType.IsExceptionType();
                _exceptionArgumentIndex = new List<int>();
                _formatArgumentIndex = new List<int>();

                ValidateSignature();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Write()
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
                _threadLogAppenderField.Void(x => x.AppendActivityNode, activityLocal);

                m.Return(activityLocal.CastTo<TT.TReturn>());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendLogNode()
            {
                var m = _underlyingWriter;

                IOperand<string> fullDetailsTextOperand;
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

                    fullDetailsTextOperand = exceptionOperand.FuncToString();
                }
                else if ( _mustCreateException )
                {
                    exceptionOperand = m.ReturnValueLocal.CastTo<Exception>();
                    fullDetailsTextOperand = exceptionOperand.FuncToString();
                }
                else
                {
                    exceptionOperand = m.Const<Exception>(null);
                    fullDetailsTextOperand = m.Const<string>(null);
                }
                
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
                    formatStringBuilder.Append(i > 0 ? ", " : ": ");
                    formatStringBuilder.AppendFormat("{0}={{{1}}}", _signature.ArgumentName[_formatArgumentIndex[i]], i);
                }

                _formatString = formatStringBuilder.ToString();
            }
        }
    }
}
