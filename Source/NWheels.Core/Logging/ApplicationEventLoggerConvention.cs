using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Logging;

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
            private readonly MethodInfo _declaration;
            private readonly LogAttributeBase _attribute;
            private readonly bool _mustCreateException;
            private readonly ParameterInfo _exceptionParameter;
            private readonly ParameterInfo[] _parameters;
            private string _formatString = null;
            private Local<object[]> _argumentArrayLocal = null;
            private IOperand<string> _logTextLocal;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LogMethodWriter(TemplateMethodWriter underlyingWriter, Field<IThreadLogAppender> threadLogAppenderField)
            {
                _underlyingWriter = underlyingWriter;
                _threadLogAppenderField = threadLogAppenderField;
                _declaration = underlyingWriter.OwnerMethod.MethodDeclaration;
                _attribute = _declaration.GetCustomAttribute<LogAttributeBase>();

                ValidateDeclaration();

                _parameters = _declaration.GetParameters();
                _exceptionParameter = _parameters.FirstOrDefault(p => p.ParameterType.IsExceptionType());
                _mustCreateException = _declaration.ReturnType.IsExceptionType();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Write()
            {
                if ( _parameters.Length > 0 )
                {
                    BuildFormatString();
                    CopyArgumentsToArray();
                    FormatLogText();
                }
                else
                {
                    _logTextLocal = _underlyingWriter.Const(_declaration.Name.SplitPascalCase());
                }

                if ( _attribute.IsActivity )
                {
                    AppendAndReturnActivityLogNode();
                }
                else
                {
                    if ( _mustCreateException )
                    {
                        CreateException();
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

                _argumentArrayLocal = m.Local(initialValue: m.NewArray<object>(length: m.Const(_declaration.GetParameters().Length)));
                m.ForEachArgument((arg, index) => _argumentArrayLocal.ItemAt(index).Assign(arg.CastTo<object>()));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FormatLogText()
            {
                var m = _underlyingWriter;
                _logTextLocal = m.Local(initialValue: Static.Func(string.Format, m.Const(_formatString), _argumentArrayLocal));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CreateException()
            {
                var m = _underlyingWriter;
                m.ReturnValueLocal = m.Local(initialValue: m.New<TypeTemplate.TReturn>(_logTextLocal));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendAndReturnActivityLogNode()
            {
                var m = _underlyingWriter;

                var activityLocal = m.Local<ActivityLogNode>(initialValue: m.New<FormattedActivityLogNode>(_logTextLocal));
                _threadLogAppenderField.Void(x => x.AppendActivityNode, activityLocal);

                m.Return(activityLocal.CastTo<TypeTemplate.TReturn>());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AppendLogNode()
            {
                var m = _underlyingWriter;

                IOperand<string> fullDetailsTextOperand;
                IOperand<Exception> exceptionOperand;

                if ( _exceptionParameter != null )
                {
                    exceptionOperand = m.Argument<Exception>(_exceptionParameter.Position + 1);
                    fullDetailsTextOperand = exceptionOperand.FuncToString();
                }
                else
                {
                    exceptionOperand = m.Const<Exception>(null);
                    fullDetailsTextOperand = m.Const<string>(null);
                }
                
                _threadLogAppenderField.Void(x => x.AppendLogNode, m.New<FormattedLogNode>(
                    m.Const(_attribute.Level),
                    _logTextLocal,
                    fullDetailsTextOperand,
                    m.Const(LogContentTypes.Text),
                    exceptionOperand));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateDeclaration()
            {
                if ( _attribute == null )
                {
                    throw NewContractConventionException(_declaration, "Log attribute is missing");
                }

                if ( !_declaration.IsVoid() && _declaration.ReturnType != typeof(ILogActivity) && !_declaration.ReturnType.IsExceptionType() )
                {
                    throw NewContractConventionException(_declaration, "Method must either be void, return ILogActivity, or return an exception type");
                }

                if ( _declaration.GetParameters().Any(p => p.ParameterType.IsByRef || p.IsOut) )
                {
                    throw NewContractConventionException(_declaration, "Method cannot have ref or out parameters");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void BuildFormatString()
            {
                var formatStringBuilder = new StringBuilder(_declaration.Name.SplitPascalCase());
                var parameters = _declaration.GetParameters();

                for ( int index = 0; index < parameters.Length; index++ )
                {
                    formatStringBuilder.Append(index > 0 ? ", " : ": ");
                    formatStringBuilder.AppendFormat("{0}={{{1}}}", parameters[index].Name, index);
                }

                _formatString = formatStringBuilder.ToString();
            }
        }
    }
}
