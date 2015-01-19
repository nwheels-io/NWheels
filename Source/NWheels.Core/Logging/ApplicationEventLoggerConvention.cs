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

        private void ImplementLogMethod(TemplateMethodWriter writer)
        {
            var declaration = writer.OwnerMethod.MethodDeclaration;
            var attribute = declaration.GetCustomAttribute<LogAttributeBase>();

            ValidateLogMethod(declaration, attribute);
            var formatString = BuildLogFormatString(declaration);

            var argumentsLocal = writer.Local(initialValue: writer.NewArray<object>(declaration.GetParameters().Length));
            writer.ForEachArgument((arg, index) => argumentsLocal.ItemAt(index).Assign(arg.CastTo<object>()));

            var messageLocal = writer.Local<string>();
            messageLocal.Assign(Static.Func(string.Format, writer.Const(formatString), argumentsLocal));

            if ( attribute.IsActivity )
            {
                _threadLogAppenderField.Void(x => x.AppendActivityNode, writer.New<FormattedActivityLogNode>(messageLocal));
            }
            else
            {
                _threadLogAppenderField.Void(x => x.AppendLogNode, writer.New<FormattedLogNode>(
                    writer.Const(attribute.Level),
                    messageLocal, 
                    writer.Const<string>(null),
                    writer.Const(LogContentTypes.Text),
                    writer.Const<Exception>(null)));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string BuildLogFormatString(MethodInfo declaration)
        {
            var formatString = new StringBuilder();
            var parameters = declaration.GetParameters();

            for ( int index = 0 ; index < parameters.Length ; index++ )
            {
                if ( index > 0 )
                {
                    formatString.Append(",");
                }
                
                formatString.AppendFormat("{0} {1}={{{2}}}", declaration.Name, parameters[index].Name, index);
            }

            return formatString.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateLogMethod(MethodInfo declaration, LogAttributeBase attribute)
        {
            if ( attribute == null )
            {
                throw NewContractConventionException(declaration, "Log attribute is missing");
            }

            if ( !declaration.IsVoid() && declaration.ReturnType != typeof(ILogActivity) && !typeof(Exception).IsAssignableFrom(declaration.ReturnType) )
            {
                throw NewContractConventionException(declaration, "Method must either be void, return ILogActivity, or return an exception type");
            }

            if ( declaration.GetParameters().Any(p => p.ParameterType.IsByRef || p.IsOut) )
            {
                throw NewContractConventionException(declaration, "Method cannot have ref or out parameters");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ContractConventionException NewContractConventionException(MemberInfo member, string message)
        {
            return new ContractConventionException(this.GetType(), TypeTemplate.Resolve<TypeTemplate.TPrimary>(), member, message);
        }
    }
}
