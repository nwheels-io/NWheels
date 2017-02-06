using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public abstract class AbstractExpression
    {
        public TypeMember Type { get; set; }

        #if false
        public AbstractExpression DOT(FieldMember field)
        {
            return new MemberExpression() {
                Target = this,
                Member = field,
                Type = field.Type
            };
        }

        public AbstractExpression DOT(PropertyMember property)
        {
            return new MemberExpression() {
                Target = this,
                Member = property,
                Type = property.PropertyType
            };
        }

        public AbstractExpression DOT(EventMember @event)
        {
            return new MemberExpression() {
                Target = this,
                Member = @event,
                Type = @event.DelegateType
            };
        }

        public AbstractExpression CALL(string methodName, params AbstractExpression[] ARGS)
        {
            var result = new MethodCallExpression() {
                Target = this,
                MethodName = methodName,
            };
            result.Arguments.AddRange(ARGS);
            return result;
        }
        #endif
    }
}
